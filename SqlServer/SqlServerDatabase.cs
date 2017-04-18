using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Castle.Core;
using Core;
using Core.Descriptions;
using Core.Interfaces;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using SqlServer.Enums;
using DataType = SqlServer.Enums.DataType;

namespace SqlServer {
    [CastleComponent("SqlServer.SqlServerDatabase", typeof(IDatabase), Lifestyle = LifestyleType.Transient)]
    public class SqlServerDatabase : IDatabase {
        private const string ConnectionStringPattern = @"Server={0};Database={1};User Id={2};Password={3};";
        private readonly string connectionString;
        private readonly Database database;
        //private ColumnCache memoryCache;

        public SqlServerDatabase(ConnectionInfo connectionInfo/*, ColumnCache cache = null*/) {
            var serverConnection = new ServerConnection(connectionInfo.HostName, connectionInfo.User,
                connectionInfo.Password);
            var server = new Server(serverConnection);

            server.Databases.Refresh();
            database = server.Databases[connectionInfo.DatabaseName];

            connectionString = string.Format(ConnectionStringPattern,
                connectionInfo.HostName, connectionInfo.DatabaseName, connectionInfo.User, connectionInfo.Password);

            //memoryCache = cache ?? Components.Instance.GetComponent<ColumnCache>();
        }

        public IList<string> GetSchemas() {
            database.Schemas.Refresh();
            var schemas = new List<string>();

            foreach (Schema schema in database.Schemas) {
                if (!schema.Owner.Equals("dbo", StringComparison.InvariantCultureIgnoreCase)) continue;
                schemas.Add(schema.Name);
            }

            return schemas;
        }

        public IList<TableDescription> GetTables() {
            database.Tables.Refresh(true);
            var tables = new List<TableDescription>();

            foreach (Table table in database.Tables) tables.Add(GetDescription(table));

            return tables;
        }

        public TableDescription GetTable(string schema, string tableName) {
            var table = database.Tables[tableName, schema];
            return table == null ? null : GetDescription(table);
        }

        public ColumnDescription GetColumn(string schema, string tableName, string columnName) {
            return GetColumns(schema, tableName, columnName).FirstOrDefault();
        }

        public IEnumerable<ColumnDescription> GetColumns(string schema, string tableName, params string[] columnNames) {
            var databaseTable = database.Tables[tableName, schema];
            if (databaseTable == null)
                return new List<ColumnDescription>();

            var tableFullName = $"{schema}.{tableName}";
            //var cachedWithTableFullName = memoryCache.Get(columnNames.Select(colName => $"{tableFullName}.{colName}").ToArray())                                                                                                     
            //                                         .ToList();

            //if (cachedWithTableFullName.Any()) {                
            //    return cachedWithTableFullName;
            //}

            var sqlQuery =
                "SELECT COLUMN_NAME, IS_NULLABLE, columnproperty(object_id(@table_fullname), column_name, 'IsIdentity'), DATA_TYPE, CHARACTER_MAXIMUM_LENGTH " +
                "FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_NAME = @table_name " +
                "AND TABLE_SCHEMA = @schema";

            if (columnNames.Any())
                sqlQuery +=
                    $" AND COLUMN_NAME IN ({string.Join(",", columnNames.Select(columnName => $"'{columnName.DoubleQuoted()}'"))})";

            var command = new SqlCommand(sqlQuery);

            var paramTableFullName = new SqlParameter {
                ParameterName = "@table_fullname",
                Value = tableFullName
            };
            command.Parameters.Add(paramTableFullName);

            var paramSchema = new SqlParameter {ParameterName = "@schema", Value = schema};
            command.Parameters.Add(paramSchema);

            var paramTableName = new SqlParameter {ParameterName = "@table_name", Value = tableName};
            command.Parameters.Add(paramTableName);

            var dataTable = ExecuteWithResults(command);
            var results = GetResults(dataTable);
            if (!results.Any()) return new List<ColumnDescription>();

            var columnDescriptions = results.Select(result => new ColumnDescription {
                Schema = schema,
                TableName = tableName,
                Name = result[0],
                AllowsNull = result[1].Equals("YES", StringComparison.InvariantCultureIgnoreCase),
                IsIdentity = result[2].Equals("1", StringComparison.InvariantCultureIgnoreCase),
                Type = result[3],
                Length = result.Count > 3
                    ? result[4].Equals("-1")
                        ? "max"
                        : result[4]
                    : null
            }).ToList();

            //foreach (var columnDescription in columnDescriptions) {
            //    memoryCache.Add($"{tableFullName}.{columnDescription.Name}", columnDescription);
            //}

            return columnDescriptions;
        }

        public IList<IndexDescription> GetIndexes() {
            var indexes = new List<IndexDescription>();

            foreach (Table table in database.Tables) {
                table.Indexes.Refresh();
                foreach (Index index in table.Indexes) {
                    if (index.IndexKeyType != IndexKeyType.None) continue;
                    indexes.Add(GetDescription(index, table.Schema, table.Name));
                }
            }

            return indexes;
        }

        public IndexDescription GetIndex(string schema, string tableName, string indexName) {
            database.Tables.Refresh();
            var table = database.Tables[tableName, schema];
            if (table == null) return null;

            table.Indexes.Refresh();
            var index = table.Indexes[indexName];
            if (index == null) return null;

            return GetDescription(index, schema, tableName);
        }

        public IList<PrimaryKeyDescription> GetPrimaryKeys() {
            database.Tables.Refresh();
            var primaryKeys = new List<PrimaryKeyDescription>();

            foreach (Table table in database.Tables) {
                var primaryKey = GetPrimaryKey(new TableDescription {Schema = table.Schema, Name = table.Name});
                if (primaryKey != null) primaryKeys.Add(primaryKey);
            }

            return primaryKeys;
        }

        public IList<ForeignKeyDescription> GetForeignKeys() {
            database.Tables.Refresh();
            var foreignKeys = new List<ForeignKeyDescription>();

            foreach (Table table in database.Tables) {
                var keys = GetForeignKeys(new TableDescription {Schema = table.Schema, Name = table.Name});
                foreach (var foreignKey in keys) foreignKeys.Add(foreignKey);
            }

            return foreignKeys;
        }

        public IList<ForeignKeyDescription> GetForeignKeysReferencing(ConstraintDescription constraintDescription) {
            var createFkReferencingTempTableCommadnText =
                "CREATE TABLE #TempTable ( " +
                "PKTABLE_QUALIFIER nvarchar(max), " +
                "PKTABLE_OWNER nvarchar(max), " +
                "PKTABLE_NAME nvarchar(max), " +
                "PKCOLUMN_NAME nvarchar(max), " +
                "FKTABLE_QUALIFIER nvarchar(max), " +
                "FKTABLE_OWNER nvarchar(max), " +
                "FKTABLE_NAME nvarchar(max), " +
                "FKCOLUMN_NAME nvarchar(max), " +
                "KEY_SEQ nvarchar(max), " +
                "UPDATE_RULE nvarchar(max), " +
                "DELETE_RULE nvarchar(max), " +
                "FK_NAME nvarchar(max), " +
                "PK_NAME nvarchar(max), " +
                "DEFERRABILITY nvarchar(max)) " +
                "INSERT INTO #TempTable " +
                "EXEC sp_fkeys @pktable_name = @table_name, @pktable_owner = @schema " +
                "SELECT FKTABLE_OWNER, FKTABLE_NAME, FK_NAME, FKCOLUMN_NAME, PKCOLUMN_NAME, PKTABLE_OWNER, PKTABLE_NAME " +
                "FROM #TempTable " +
                "WHERE PK_NAME = @constraint_name " +
                "DROP TABLE #TempTable";

            var command = new SqlCommand(createFkReferencingTempTableCommadnText);

            var paramSchema = new SqlParameter {ParameterName = "@schema", Value = constraintDescription.Schema};
            command.Parameters.Add(paramSchema);

            var paramTableName = new SqlParameter {
                ParameterName = "@table_name",
                Value = constraintDescription.TableName
            };
            command.Parameters.Add(paramTableName);

            var paramConstraintName = new SqlParameter {
                ParameterName = "@constraint_name",
                Value = constraintDescription.Name
            };
            command.Parameters.Add(paramConstraintName);

            var foreignKeys = new List<ForeignKeyDescription>();
            var dataTable = ExecuteWithResults(command);
            var results = GetResults(dataTable);
            if (!results.Any()) return foreignKeys;

            foreach (var result in results)
                foreignKeys.Add(new ForeignKeyDescription {
                    Schema = result[0],
                    TableName = result[1],
                    Name = result[2],
                    Columns =
                        new Dictionary<string, ColumnDescription> {
                            {result[3], GetColumn(result[5], result[6], result[4])}
                        }
                });

            return foreignKeys;
        }

        public IList<ForeignKeyDescription> GetForeignKeysReferencing(ColumnDescription columnDescription) {
            var createTempFkTableCommandText = 
                "CREATE TABLE #TempTable ( " +
                "PKTABLE_QUALIFIER nvarchar(max), " +
                "PKTABLE_OWNER nvarchar(max), " +
                "PKTABLE_NAME nvarchar(max), " +
                "PKCOLUMN_NAME nvarchar(max), " +
                "FKTABLE_QUALIFIER nvarchar(max), " +
                "FKTABLE_OWNER nvarchar(max), " +
                "FKTABLE_NAME nvarchar(max), " +
                "FKCOLUMN_NAME nvarchar(max), " +
                "KEY_SEQ nvarchar(max), " +
                "UPDATE_RULE nvarchar(max), " +
                "DELETE_RULE nvarchar(max), " +
                "FK_NAME nvarchar(max), " +
                "PK_NAME nvarchar(max), " +
                "DEFERRABILITY nvarchar(max)) " +
                "INSERT INTO #TempTable " +
                "EXEC sp_fkeys @fktable_name = @table_name, @fktable_owner = @schema  " +
                "SELECT FKTABLE_OWNER, FKTABLE_NAME, FK_NAME, FKCOLUMN_NAME, PKCOLUMN_NAME, PKTABLE_OWNER, PKTABLE_NAME " +
                "FROM #TempTable " +
                "WHERE FKCOLUMN_NAME = @column_name " +
                "DROP TABLE #TempTable";

            var command = new SqlCommand(createTempFkTableCommandText);

            var paramSchema = new SqlParameter {ParameterName = "@schema", Value = columnDescription.Schema};
            command.Parameters.Add(paramSchema);

            var paramTableName = new SqlParameter {ParameterName = "@table_name", Value = columnDescription.TableName};
            command.Parameters.Add(paramTableName);

            var paramColumnName = new SqlParameter {ParameterName = "@column_name", Value = columnDescription.Name};
            command.Parameters.Add(paramColumnName);

            var foreignKeys = new List<ForeignKeyDescription>();
            var dataTable = ExecuteWithResults(command);
            var results = GetResults(dataTable);
            if (!results.Any()) return foreignKeys;

            foreach (var result in results) {
                var schema = result[0];
                var tableName = result[1];
                var fkName = result[2];

                var fkColumnName = result[3];
                var fkColumnRefSchemaName = result[5];
                var fkColumnRefTableName = result[6];
                var fkColumnRefName = result[4];

                foreignKeys.Add(new ForeignKeyDescription {
                    Schema = schema,
                    TableName = tableName,
                    Name = fkName,
                    Columns =
                        new Dictionary<string, ColumnDescription> {
                            {fkColumnName, GetColumn(fkColumnRefSchemaName, fkColumnRefTableName, fkColumnRefName)}
                        }
                });
            }

            return foreignKeys;
        }

        public IList<UniqueDescription> GetUniqueKeys() {
            database.Tables.Refresh();
            var uniqueKeys = new List<UniqueDescription>();

            foreach (Table table in database.Tables) {
                var uniques = GetUniqueKeys(new TableDescription {Schema = table.Schema, Name = table.Name});
                foreach (var uniqueKey in uniques) uniqueKeys.Add(uniqueKey);
            }

            return uniqueKeys;
        }

        public IList<DefaultDescription> GetDefaults() {
            var selectDefaults =
                "SELECT schemas.name, tables.name, all_columns.name, default_constraints.name, default_constraints.definition " +
                "FROM sys.all_columns " +
                "INNER JOIN sys.tables ON all_columns.object_id = tables.object_id " +
                "INNER JOIN sys.schemas ON tables.schema_id = schemas.schema_id " +
                "INNER JOIN sys.default_constraints ON all_columns.default_object_id = default_constraints.object_id";

            var command = new SqlCommand(selectDefaults);

            var dataTable = ExecuteWithResults(command);
            var results = GetResults(dataTable);

            var defaults = new List<DefaultDescription>();
            if (!results.Any()) return defaults;

            foreach (var result in results) {
                var tableName = result.ElementAt(1);

                defaults.Add(new DefaultDescription {
                    Schema = result.First(),
                    TableName = tableName,
                    Column = GetColumn(result.First(), tableName, result.ElementAt(2)),
                    Name = result.ElementAt(3),
                    DefaultValue = result.ElementAt(4)
                });
            }

            return defaults;
        }

        public IList<IndexDescription> GetIndexes(string schema, string tableName) {
            var table = database.Tables[tableName, schema];

            IList<IndexDescription> indexes = new List<IndexDescription>();
            if (table == null) return indexes;

            table.Indexes.Refresh();
            foreach (Index index in table.Indexes) {
                var indexDescription = GetIndex(schema, tableName, index.Name);
                indexes.Add(indexDescription);
            }

            return indexes;
        }

        public PrimaryKeyDescription GetPrimaryKey(TableDescription table) {
            var queryPk = "SELECT Col.CONSTRAINT_NAME, Col.COLUMN_NAME " +
                          "FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab, " +
                          "INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col " +
                          "WHERE Col.Constraint_Name = Tab.Constraint_Name " +
                          "AND Col.Table_Name = Tab.Table_Name " + 
                          "AND Constraint_Type = 'PRIMARY KEY' " +
                          "AND Col.Table_Schema = @schema " +
                          "AND Col.Table_Name = @table_name";

            var command = new SqlCommand(queryPk);

            var paramSchema = new SqlParameter {ParameterName = "@schema", Value = table.Schema};
            command.Parameters.Add(paramSchema);

            var paramTableName = new SqlParameter {ParameterName = "@table_name", Value = table.Name};
            command.Parameters.Add(paramTableName);

            var dataTable = ExecuteWithResults(command);
            var results = GetResults(dataTable);
            if (!results.Any()) return null;

            var pkName = results.First().First();

            var primaryKey = new PrimaryKeyDescription {
                Schema = table.Schema,
                TableName = table.Name,
                Name = pkName
            };

            foreach (var result in results) {
                var columnName = result.ElementAt(1);
                primaryKey.Columns.Add(GetColumn(table.Schema, table.Name, columnName));
            }

            return primaryKey;
        }

        public IList<ForeignKeyDescription> GetForeignKeys(TableDescription tableDescription) {
            var createTempPksTable = "CREATE TABLE #TempTable (" + 
                                     "PKTABLE_QUALIFIER nvarchar(max), " +
                                     "PKTABLE_OWNER nvarchar(max), " +
                                     "PKTABLE_NAME nvarchar(max), " +
                                     "PKCOLUMN_NAME nvarchar(max), " +
                                     "FKTABLE_QUALIFIER nvarchar(max), " +
                                     "FKTABLE_OWNER nvarchar(max), " +
                                     "FKTABLE_NAME nvarchar(max), " +
                                     "FKCOLUMN_NAME nvarchar(max), " +
                                     "KEY_SEQ nvarchar(max), " +
                                     "UPDATE_RULE nvarchar(max), " +
                                     "DELETE_RULE nvarchar(max), " +
                                     "FK_NAME nvarchar(max), " +
                                     "PK_NAME nvarchar(max), " +
                                     "DEFERRABILITY nvarchar(max)) " +            
                                     "INSERT INTO #TempTable " +
                                     "EXEC sp_fkeys @fktable_name = @table_name, @fktable_owner = @schema " +                
                                     "SELECT FK_NAME, FKCOLUMN_NAME, PKCOLUMN_NAME, FKTABLE_OWNER, FKTABLE_NAME, PKTABLE_NAME, PKTABLE_OWNER " +
                                     "FROM #TempTable " +
                                     "DROP TABLE #TempTable";

            var command = new SqlCommand(createTempPksTable);

            var paramSchema = new SqlParameter {ParameterName = "@schema", Value = tableDescription.Schema};
            command.Parameters.Add(paramSchema);

            var paramTableName = new SqlParameter {ParameterName = "@table_name", Value = tableDescription.Name};
            command.Parameters.Add(paramTableName);

            var foreignKeys = new List<ForeignKeyDescription>();
            var dataTable = ExecuteWithResults(command);
            var results = GetResults(dataTable);
            if (!results.Any()) return foreignKeys;

            foreach (var result in results) {
                ForeignKeyDescription foreignKey;
                var fkName = result.First();
                var name = result.ElementAt(1);
                var columnName = result.ElementAt(2);
                var fkSchemaName = result.ElementAt(3);
                var fkTableReferenceName = result.ElementAt(4);
                var tableName = result.ElementAt(5);
                var schema = result.ElementAt(6);

                if (foreignKeys.Any(fk => fk.Name.Equals(fkName))) {
                    foreignKey = foreignKeys.Single(fk => fk.Name.Equals(fkName));
                }
                else {
                    
                    foreignKey = new ForeignKeyDescription {
                        Name = fkName,
                        TableName = fkTableReferenceName,
                        Schema = fkSchemaName,
                        Columns = new Dictionary<string, ColumnDescription>()
                    };
                    foreignKeys.Add(foreignKey);
                }

                foreignKey.Columns.Add(new KeyValuePair<string, ColumnDescription>(name, 
                                                                                   GetColumn(schema, 
                                                                                             tableName, 
                                                                                             columnName)));
            }

            return foreignKeys;
        }

        public IList<UniqueDescription> GetUniqueKeys(TableDescription tableDescription) {
            var queryUniques = "SELECT Col.CONSTRAINT_NAME, Col.COLUMN_NAME FROM " +
                               "INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab, " +
                               "INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col " +
                               "WHERE Col.Constraint_Name = Tab.Constraint_Name " +
                               "AND Col.Table_Name = Tab.Table_Name " +
                               "AND Constraint_Type = 'UNIQUE' " +
                               "AND Col.Table_Schema = @schema " +
                               "AND Col.Table_Name = @table_name";

            var command = new SqlCommand(queryUniques);

            var paramSchema = new SqlParameter {ParameterName = "@schema", Value = tableDescription.Schema};
            command.Parameters.Add(paramSchema);

            var paramTableName = new SqlParameter {ParameterName = "@table_name", Value = tableDescription.Name};
            command.Parameters.Add(paramTableName);

            var dataTable = ExecuteWithResults(command);
            var results = GetResults(dataTable);
            var uniqueKeys = new List<UniqueDescription>();
            if (!results.Any()) return uniqueKeys;

            var uniques = results.GroupBy(result => result.First(),
                (name, groups) => new {Name = name, Columns = groups.Select(group => group.ElementAt(1))});

            foreach (var unique in uniques) {
                UniqueDescription uniqueKey;
                if (uniqueKeys.Any(f => f.Name.Equals(unique.Name))) {
                    uniqueKey = uniqueKeys.Single(f => f.Name.Equals(unique.Name));
                }
                else {
                    uniqueKey = new UniqueDescription {
                        Name = unique.Name,
                        TableName = tableDescription.Name,
                        Schema = tableDescription.Schema
                    };
                    uniqueKeys.Add(uniqueKey);
                }
                uniqueKey.Columns.AddRange(GetColumns(tableDescription.Schema, tableDescription.Name,
                    unique.Columns.ToArray()));
            }

            return uniqueKeys;
        }

        public UniqueDescription GetUniqueKey(string uniqueKeyName, string schema) {
            var selectUniqueKey =
                "SELECT Col.TABLE_NAME, Col.COLUMN_NAME " +
                "FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab," +
                "INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col " +
                "WHERE Col.Constraint_Name = Tab.Constraint_Name " +
                "AND Col.Table_Name = Tab.Table_Name " +
                "AND Constraint_Type = 'UNIQUE' " +
                "AND Col.Constraint_Name = @unique_name " +
                "AND Col.TABLE_SCHEMA = @schema";

            var command = new SqlCommand(selectUniqueKey);

            var paramSchema = new SqlParameter {ParameterName = "@schema", Value = schema};
            command.Parameters.Add(paramSchema);

            var paramUniqueName = new SqlParameter {ParameterName = "@unique_name", Value = uniqueKeyName};
            command.Parameters.Add(paramUniqueName);

            var dataTable = ExecuteWithResults(command);
            var results = GetResults(dataTable);
            if (!results.Any()) return null;

            var tableName = results.First().First();
            var uniqueKey = new UniqueDescription {
                Name = uniqueKeyName,
                Schema = schema,
                TableName = tableName
            };

            var columnNames = results.Select(result => result.ElementAt(1)).ToArray();
            uniqueKey.Columns.AddRange(GetColumns(schema, tableName, columnNames));

            return uniqueKey;
        }

        public DefaultDescription GetDefault(string defaultName, string schema) {
            var delectDefault = "SELECT tables.name, all_columns.name, default_constraints.definition " +
                                "FROM sys.all_columns " +
                                "INNER JOIN sys.tables ON all_columns.object_id = tables.object_id " +
                                "INNER JOIN sys.schemas ON tables.schema_id = schemas.schema_id " +
                                "INNER JOIN sys.default_constraints ON all_columns.default_object_id = default_constraints.object_id " +
                                "WHERE default_constraints.name LIKE @default_name " +
                                "AND schemas.name LIKE @schema";

            var command = new SqlCommand(delectDefault);

            var paramSchema = new SqlParameter {ParameterName = "@schema", Value = schema};
            command.Parameters.Add(paramSchema);

            var paramDefaultName = new SqlParameter {ParameterName = "@default_name", Value = defaultName};
            command.Parameters.Add(paramDefaultName);

            var dataTable = ExecuteWithResults(command);
            var results = GetResults(dataTable);
            if (!results.Any()) return null;

            var tableName = results.First().First();
            return new DefaultDescription {
                Schema = schema,
                TableName = tableName,
                Column = GetColumn(schema, tableName, results.First().ElementAt(1)),
                DefaultValue = results.First().ElementAt(2),
                Name = defaultName
            };
        }

        private TableDescription GetDescription(Table table) {
            var tableDescription = new TableDescription {
                Name = table.Name,
                Schema = table.Schema
            };

            var tableColumnDescriptions = GetColumns(table.Schema, table.Name);
            tableDescription.Columns.AddRange(tableColumnDescriptions);

            return tableDescription;
        }

        private IndexDescription GetDescription(Index index, string schema, string tableName) {
            var indexDescription = new IndexDescription {
                Schema = schema,
                TableName = tableName,
                Name = index.Name,
                Unique = index.IsUnique
            };

            index.IndexedColumns.Refresh();

            var columnDescriptions = GetColumns(schema, tableName,
                index.IndexedColumns.Cast<IndexedColumn>().Select(col => col.Name).ToArray());
            indexDescription.Columns.AddRange(columnDescriptions);

            return indexDescription;
        }

        public PrimaryKeyDescription GetPrimaryKey(string primaryKeyName, string schema = "dbo") {
            var queryPk = "SELECT Col.Table_Name, Col.COLUMN_NAME " +
                          "FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab," +
                          "INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col " +
                          "WHERE Col.Constraint_Name = @pk_name " +
                          "AND Constraint_Type = 'PRIMARY KEY' " +
                          "AND Col.Table_Schema = @schema";

            var command = new SqlCommand(queryPk);

            var paramSchema = new SqlParameter {ParameterName = "@schema", Value = schema};
            command.Parameters.Add(paramSchema);

            var paramPrimaryName = new SqlParameter {ParameterName = "@pk_name", Value = primaryKeyName};
            command.Parameters.Add(paramPrimaryName);

            var dataTable = ExecuteWithResults(command);
            var results = GetResults(dataTable);
            if (!results.Any()) return null;

            var tableName = results.First().First();
            var primaryKey = new PrimaryKeyDescription {Schema = schema, TableName = tableName, Name = primaryKeyName};
            var columnNames = results.Select(tuple => tuple.ElementAt(1)).ToArray();

            primaryKey.Columns.AddRange(GetColumns(schema, tableName, columnNames));

            return primaryKey;
        }

        internal bool DataTypeIsValid(string dataTypeName) {
            var dataTypes = Enums.Enums.GetDefaultValues<DataType>();
            return dataTypes.Any(
                dataType => dataType.Equals(dataTypeName, StringComparison.InvariantCultureIgnoreCase));
        }

        internal bool TableHasDuplicatedValuesForColumns(string schema, string tableName, IList<string> columnNames) {
            foreach (var columnName in columnNames)
                if (!ColumnExists(schema, tableName, columnName))
                    throw new ArgumentException();

            var query = "SELECT COUNT(*) " +
                        $"FROM [{schema}].[{tableName}] " +
                        $"GROUP BY {string.Join(",", columnNames)} " +
                        "HAVING COUNT(*) > 1";

            var dataSet = database.ExecuteWithResults(query);
            var results = GetResults(dataSet);
            return results.Any();
        }

        internal bool IdentifierNameIsValid(string identifierName) {
            if (string.IsNullOrWhiteSpace(identifierName) || identifierName.Length > 128) return false;

            var firstCharacter = identifierName[0].ToString();
            if (!Regex.IsMatch(firstCharacter, @"[a-zA-Z_@#]")) return false;

            if (!Regex.IsMatch(identifierName, @"^[a-zA-Z0-9_@#$']*$")) return false;

            var keywords = Enums.Enums.GetDescriptions<Keyword>();
            if (keywords.Any(keyword => keyword.Equals(identifierName,
                StringComparison.InvariantCultureIgnoreCase))) return false;

            return true;
        }

        private bool ColumnExists(string schema, string tableName, string columnName) {
            var table = database.Tables[tableName, schema];
            if (string.IsNullOrWhiteSpace(columnName))
                return false;
            var coluna = table?.Columns[columnName];
            return coluna != null;
        }

        private DataTable ExecuteWithResults(SqlCommand command) {
            command.Connection = new SqlConnection {ConnectionString = $"{connectionString}; Connection Timeout=0"};
            var dataTable = new DataTable();

            using (command.Connection) {
                using (command) {
                    command.Connection.Open();

                    using (var reader = command.ExecuteReader()) {
                        dataTable.Load(reader);
                    }
                }
            }

            return dataTable;
        }

        internal void ExecuteNonQuery(SqlCommand command) {
            command.Connection = new SqlConnection {ConnectionString = connectionString};

            using (command.Connection)
            using (command) {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private List<List<string>> GetResults(DataTable dataTable) {
            var rowCollection = dataTable.Rows;

            var results = new List<List<string>>();
            if (rowCollection.Count == 0) return results;

            foreach (var row in rowCollection) {
                var dataRow = row as DataRow;
                if (dataRow == null) continue;

                var result = new List<string>();
                foreach (var item in dataRow.ItemArray) {
                    var itemValue = item as string ?? item.ToString();
                    result.Add(itemValue);
                }

                if (result.Any()) results.Add(result);
            }

            return results;
        }

        private List<List<string>> GetResults(DataSet dataSet) {
            return GetResults(dataSet.Tables["Table"]);
        }

        public bool SchemaExists(string schemaName) {
            database.Schemas.Refresh();
            var schema = database.Schemas[schemaName];
            return schema != null;
        }

        internal void ResetCache() {
            //memoryCache.Reset();
        }
    }
}