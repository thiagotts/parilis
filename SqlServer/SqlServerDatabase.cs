using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Descriptions;
using Core.Interfaces;
using Microsoft.SqlServer.Management.Smo;
using SqlServer.Enums;
using DataType = SqlServer.Enums.DataType;

namespace SqlServer {
    public class SqlServerDatabase : IDatabase {
        private readonly Database database;

        public SqlServerDatabase(Database database) {
            this.database = database;
        }

        public IList<TableDescription> GetTables() {
            database.Tables.Refresh();
            var tables = new List<TableDescription>();

            foreach (Table table in database.Tables) {
                tables.Add(GetDescription(table));
            }

            return tables;
        }

        public TableDescription GetTable(string schema, string tableName) {
            database.Tables.Refresh();
            var table = database.Tables[tableName, schema];
            return table == null ? null : GetDescription(table);
        }

        public ColumnDescription GetColumn(string schema, string tableName, string columnName) {
            if (!ColumnExists(schema, tableName, columnName)) return null;

            var query = string.Format(@"SELECT IS_NULLABLE, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
                                        FROM INFORMATION_SCHEMA.COLUMNS
                                        WHERE TABLE_NAME = '{0}'
                                        AND TABLE_SCHEMA = '{1}'
                                        AND COLUMN_NAME = '{2}'", tableName, schema, columnName);

            var dataSet = database.ExecuteWithResults(query);
            var results = GetResults(dataSet);
            if (!results.Any()) return null;

            return new ColumnDescription {
                Schema = schema,
                TableName = tableName,
                Name = columnName,
                AllowsNull = results[0][0].Equals("YES", StringComparison.InvariantCultureIgnoreCase),
                Type = results[0][1],
                MaximumSize = results[0].Count > 2 ?
                    results[0][2].Equals("-1") ? "max" : results[0][2]
                    : null,
            };
        }

        public IList<IndexDescription> GetIndexes() {
            throw new NotImplementedException();
        }

        public IList<IndexDescription> GetIndexes(string schema, string tableName) {
            var table = database.Tables[tableName, schema];

            IList<IndexDescription> indexes = new List<IndexDescription>();
            if (table == null) return indexes;

            foreach (Index index in table.Indexes) {
                var indexDescription = GetIndex(schema, tableName, index.Name);
                indexes.Add(indexDescription);
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

            var indexDescription = new IndexDescription {
                Schema = schema,
                TableName = tableName,
                Name = indexName,
                Unique = index.IsUnique,
                ColumnNames = new List<string>()
            };

            foreach (IndexedColumn indexedColumn in index.IndexedColumns) {
                indexDescription.ColumnNames.Add(indexedColumn.Name);
            }

            return indexDescription;
        }

        public IList<PrimaryKeyDescription> GetPrimaryKeys() {
            throw new NotImplementedException();
        }

        public PrimaryKeyDescription GetPrimaryKey(TableDescription table) {
            var dataSet = database.ExecuteWithResults(string.Format(@"
                SELECT Col.CONSTRAINT_NAME, Col.COLUMN_NAME FROM
                    INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab,
                    INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col
                WHERE
                    Col.Constraint_Name = Tab.Constraint_Name
                    AND Col.Table_Name = Tab.Table_Name
                    AND Constraint_Type = 'PRIMARY KEY'
                    AND Col.Table_Schema = '{0}'
                    AND Col.Table_Name = '{1}'", table.Schema, table.Name));

            var results = GetResults(dataSet);
            if (!results.Any()) return null;

            var primaryKey = new PrimaryKeyDescription {Schema = table.Schema, TableName = table.Name, Name = results[0][0]};
            primaryKey.ColumnNames = new List<string>();

            foreach (var result in results) {
                primaryKey.ColumnNames.Add(result[1]);
            }

            return primaryKey;
        }

        public IList<ForeignKeyDescription> GetForeignKeys() {
            throw new NotImplementedException();
        }

        public IList<ForeignKeyDescription> GetForeignKeys(TableDescription tableDescription) {
            var dataSet = database.ExecuteWithResults(string.Format(@"
                CREATE TABLE #TempTable (
                 PKTABLE_QUALIFIER nvarchar(max),
                 PKTABLE_OWNER nvarchar(max),
                 PKTABLE_NAME nvarchar(max),
                 PKCOLUMN_NAME nvarchar(max),
                 FKTABLE_QUALIFIER nvarchar(max),
                 FKTABLE_OWNER nvarchar(max),
                 FKTABLE_NAME nvarchar(max),
                 FKCOLUMN_NAME nvarchar(max),
                 KEY_SEQ nvarchar(max),
                 UPDATE_RULE nvarchar(max),
                 DELETE_RULE nvarchar(max),  
                 FK_NAME nvarchar(max),
                 PK_NAME nvarchar(max),
                 DEFERRABILITY nvarchar(max))                 
                INSERT INTO #TempTable
                EXEC sp_fkeys @fktable_name = N'{0}', @fktable_owner = N'{1}'                
                SELECT FK_NAME, FKCOLUMN_NAME, PKCOLUMN_NAME, FKTABLE_OWNER, FKTABLE_NAME, PKTABLE_NAME, PKTABLE_OWNER
                FROM #TempTable
                DROP TABLE #TempTable
                ",
                tableDescription.Name, tableDescription.Schema));

            var foreignKeys = new List<ForeignKeyDescription>();
            var results = GetResults(dataSet);
            if (!results.Any()) return foreignKeys;

            foreach (var result in results) {
                ForeignKeyDescription foreignKey;
                if (foreignKeys.Any(f => f.Name.Equals(result[0]))) {
                    foreignKey = foreignKeys.Single(f => f.Name.Equals(result[0]));
                }
                else {
                    foreignKey = new ForeignKeyDescription {Name = result[0], TableName = result[4], Schema = result[3], Columns = new Dictionary<string, ColumnDescription>()};
                    foreignKeys.Add(foreignKey);
                }

                foreignKey.Columns.Add(new KeyValuePair<string, ColumnDescription>(result[1], new ColumnDescription {
                    Name = result[2],
                    TableName = result[5],
                    Schema = result[6]
                }));
            }

            return foreignKeys;
        }

        public IList<ForeignKeyDescription> GetForeignKeysReferencing(ConstraintDescription primaryKeyDescription) {
            var dataSet = database.ExecuteWithResults(string.Format(@"
                CREATE TABLE #TempTable (
                 PKTABLE_QUALIFIER nvarchar(max),
                 PKTABLE_OWNER nvarchar(max),
                 PKTABLE_NAME nvarchar(max),
                 PKCOLUMN_NAME nvarchar(max),
                 FKTABLE_QUALIFIER nvarchar(max),
                 FKTABLE_OWNER nvarchar(max),
                 FKTABLE_NAME nvarchar(max),
                 FKCOLUMN_NAME nvarchar(max),
                 KEY_SEQ nvarchar(max),
                 UPDATE_RULE nvarchar(max),
                 DELETE_RULE nvarchar(max),  
                 FK_NAME nvarchar(max),
                 PK_NAME nvarchar(max),
                 DEFERRABILITY nvarchar(max))                 
                INSERT INTO #TempTable
                EXEC sp_fkeys @pktable_name = N'{0}', @pktable_owner = N'{1}'                
                SELECT FKTABLE_OWNER, FKTABLE_NAME, FK_NAME, FKCOLUMN_NAME, PKCOLUMN_NAME
                FROM #TempTable
                WHERE PK_NAME = '{2}'
                DROP TABLE #TempTable
                ",
                primaryKeyDescription.TableName, primaryKeyDescription.Schema, primaryKeyDescription.Name));

            var foreignKeys = new List<ForeignKeyDescription>();

            var results = GetResults(dataSet);
            if (!results.Any()) return foreignKeys;

            foreach (var result in results) {
                foreignKeys.Add(new ForeignKeyDescription {
                    Schema = result[0],
                    TableName = result[1],
                    Name = result[2],
                    Columns = new Dictionary<string, ColumnDescription> {
                        {
                            result[3],
                            new ColumnDescription {
                                Name = result[4],
                                AllowsNull = false,
                                Schema = primaryKeyDescription.Schema,
                                TableName = primaryKeyDescription.TableName
                            }
                        }
                    }
                });
            }

            return foreignKeys;
        }

        public IList<UniqueDescription> GetUniqueKeys() {
            throw new NotImplementedException();
        }

        public IList<UniqueDescription> GetUniqueKeys(TableDescription tableDescription) {
            var dataSet = database.ExecuteWithResults(string.Format(@"
                SELECT Col.CONSTRAINT_NAME, Col.COLUMN_NAME FROM
                    INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab,
                    INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col
                WHERE
                    Col.Constraint_Name = Tab.Constraint_Name
                    AND Col.Table_Name = Tab.Table_Name
                    AND Constraint_Type = 'UNIQUE'
                    AND Col.Table_Schema = '{0}'
                    AND Col.Table_Name = '{1}'", tableDescription.Schema, tableDescription.Name));

            var results = GetResults(dataSet);

            var uniqueKeys = new List<UniqueDescription>();
            if (!results.Any()) return uniqueKeys;

            foreach (var result in results) {
                UniqueDescription uniqueKey;
                if (uniqueKeys.Any(f => f.Name.Equals(result[0]))) {
                    uniqueKey = uniqueKeys.Single(f => f.Name.Equals(result[0]));
                }
                else {
                    uniqueKey = new UniqueDescription {
                        Name = result[0],
                        TableName = tableDescription.Name,
                        Schema = tableDescription.Schema,
                        ColumnNames = new List<string>()
                    };
                    uniqueKeys.Add(uniqueKey);
                }

                uniqueKey.ColumnNames.Add(result[1]);
            }

            return uniqueKeys;
        }

        public UniqueDescription GetUniqueKey(string uniqueKeyName) {
            var dataSet = database.ExecuteWithResults(string.Format(@"
                SELECT Col.TABLE_SCHEMA, Col.TABLE_NAME, Col.COLUMN_NAME FROM
                    INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab,
                    INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col
                WHERE
                    Col.Constraint_Name = Tab.Constraint_Name
                    AND Col.Table_Name = Tab.Table_Name
                    AND Constraint_Type = 'UNIQUE'
                    AND Col.Constraint_Name = '{0}'", uniqueKeyName));

            var results = GetResults(dataSet);
            if (!results.Any()) return null;

            var uniqueKey = new UniqueDescription {
                Name = uniqueKeyName,
                Schema = results[0][0],
                TableName = results[0][1],
                ColumnNames = new List<string>()
            };

            foreach (var result in results) {
                uniqueKey.ColumnNames.Add(result[2]);
            }

            return uniqueKey;
        }

        public IList<DefaultDescription> GetDefaults() {
            const string query = @"SELECT schemas.name, tables.name, all_columns.name, default_constraints.name, default_constraints.definition
                                   FROM sys.all_columns
                                   INNER JOIN sys.tables ON all_columns.object_id = tables.object_id
                                   INNER JOIN sys.schemas ON tables.schema_id = schemas.schema_id
                                   INNER JOIN sys.default_constraints ON all_columns.default_object_id = default_constraints.object_id";

            var dataSet = database.ExecuteWithResults(query);
            var results = GetResults(dataSet);

            var defaults = new List<DefaultDescription>();
            if (!results.Any()) return defaults;

            foreach (var result in results) {
                defaults.Add(new DefaultDescription {
                    Schema = result[0],
                    TableName = result[1],
                    ColumnName = result[2],
                    Name = result[3],
                    DefaultValue = result[4]
                });
            }

            return defaults;
        }

        public DefaultDescription GetDefault(string defaultName, string schema) {
            var query = string.Format(@"SELECT tables.name, all_columns.name, default_constraints.definition
                                        FROM sys.all_columns
                                        INNER JOIN sys.tables ON all_columns.object_id = tables.object_id
                                        INNER JOIN sys.schemas ON tables.schema_id = schemas.schema_id
                                        INNER JOIN sys.default_constraints ON all_columns.default_object_id = default_constraints.object_id
                                        WHERE default_constraints.name LIKE '{0}'
                                        AND schemas.name LIKE '{1}'", defaultName, schema);

            var dataSet = database.ExecuteWithResults(query);
            var results = GetResults(dataSet);
            if (!results.Any()) return null;

            return new DefaultDescription {
                Schema = schema,
                TableName = results[0][0],
                ColumnName = results[0][1],
                DefaultValue = results[0][2],
                Name = defaultName
            };
        }

        private TableDescription GetDescription(Table table) {
            var tableDescription = new TableDescription {
                Name = table.Name,
                Schema = table.Schema,
                Columns = new List<ColumnDescription>()
            };

            foreach (Column column in table.Columns) {
                tableDescription.Columns.Add(GetColumn(table.Schema, table.Name, column.Name));
            }

            return tableDescription;
        }

        public PrimaryKeyDescription GetPrimaryKey(string primaryKeyName, string schema = "dbo") {
            var dataSet = database.ExecuteWithResults(string.Format(@"
                SELECT Col.Table_Name, Col.COLUMN_NAME FROM
                    INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab,
                    INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col
                WHERE
                    Col.Constraint_Name = '{0}'
                    AND Constraint_Type = 'PRIMARY KEY'
                    AND Col.Table_Schema = '{1}'", primaryKeyName, schema));

            var results = GetResults(dataSet);
            if (!results.Any()) return null;

            var primaryKey = new PrimaryKeyDescription {Schema = schema, TableName = results[0][0], Name = primaryKeyName};
            primaryKey.ColumnNames = new List<string>();

            foreach (var result in results) {
                primaryKey.ColumnNames.Add(result[1]);
            }

            return primaryKey;
        }

        internal bool DataTypeIsValid(string dataType) {
            var dataTypes = Enums.Enums.GetDefaultValues<DataType>();
            return dataTypes.Any(t => t.Equals(dataType, StringComparison.InvariantCultureIgnoreCase));
        }

        internal bool ColumnHasDuplicatedValues(ColumnDescription column) {
            if (!ColumnExists(column.Schema, column.TableName, column.Name))
                throw new ArgumentException();

            var query = string.Format(@"SELECT [{0}], COUNT(*)
                                        FROM [{1}].[{2}]
                                        GROUP BY [{0}]
                                        HAVING COUNT(*) > 1", column.Name, column.Schema, column.TableName);

            var dataSet = database.ExecuteWithResults(query);
            var results = GetResults(dataSet);
            return results.Any();
        }

        internal bool IdentifierNameIsValid(string identifierName) {
            if (string.IsNullOrWhiteSpace(identifierName) || identifierName.Length > 128) return false;

            var firstCharacter = identifierName[0].ToString();
            if (!Regex.IsMatch(firstCharacter, @"[a-zA-Z_@#]")) return false;

            if (!Regex.IsMatch(identifierName, @"^[a-zA-Z0-9_@#$]*$")) return false;

            var keywords = Enums.Enums.GetDescriptions<Keyword>();
            if (keywords.Any(k => k.Equals(identifierName, StringComparison.InvariantCultureIgnoreCase))) return false;

            return true;
        }

        private bool ColumnExists(string schema, string tableName, string columnName) {
            var table = database.Tables[tableName, schema];
            if (table == null) return false;

            table.Columns.Refresh(true);
            var coluna = table.Columns[columnName];
            return coluna != null;
        }

        private List<List<string>> GetResults(DataSet dataSet) {
            var rowCollection = dataSet.Tables["Table"].Rows;

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

        public bool SchemaExists(string schemaName) {
            database.Schemas.Refresh();
            var schema = database.Schemas[schemaName];
            return schema != null;
        }
    }
}