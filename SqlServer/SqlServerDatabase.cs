using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.Descriptions;
using Core.Interfaces;
using Microsoft.SqlServer.Management.Smo;

namespace SqlServer {
    public class SqlServerDatabase : IDatabase {
        private readonly Database database;

        public SqlServerDatabase(Database database) {
            this.database = database;
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

        public IList<ForeignKeyDescription> GetForeignKeysReferencing(TableDescription tableDescription) {
            var dataSet = RunSpFKeys(tableDescription.Name, tableDescription.Schema);
            var foreignKeys = new List<ForeignKeyDescription>();

            var results = GetResults(dataSet);
            if (!results.Any()) return foreignKeys;

            foreach (var result in results) {
                foreignKeys.Add(new ForeignKeyDescription {
                    Schema = result[0],
                    TableName = result[1],
                    Name = result[2],
                    ColumnName = result[3],
                    ReferenceColumn = new ColumnDescription {
                        Name = result[4],
                        AllowsNull = false,
                        Schema = tableDescription.Schema,
                        TableName = tableDescription.Name
                    }
                });
            }

            return foreignKeys;
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
                    var itemValue = item as string;
                    if (itemValue == null) continue;
                    result.Add(itemValue);
                }

                if (result.Any()) results.Add(result);
            }

            return results;
        }

        private DataSet RunSpFKeys(string tableName, string schema) {
            return database.ExecuteWithResults(string.Format(@"
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
                DROP TABLE #TempTable
                ",
                tableName, schema));
        }
    }
}