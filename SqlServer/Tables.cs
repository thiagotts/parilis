using System.Collections.Generic;
using System.Linq;
using Core.Descriptions;
using Core.Exceptions;
using Core.Interfaces;
using Microsoft.SqlServer.Management.Smo;

namespace SqlServer {
    public class Tables : ITable {
        private readonly Database database;
        private readonly SqlServerDatabase sqlServerDatabase;

        public Tables(Database database) {
            this.database = database;
            sqlServerDatabase = new SqlServerDatabase(database);
        }

        public void Create(TableDescription tableDescription) {
            if (!TableNameIsValid(tableDescription))
                throw new InvalidTableNameException();

            var columns = new List<string>();
            foreach (var column in tableDescription.Columns ?? new List<ColumnDescription>()) {
                columns.Add(string.Format("[{0}] [{1}]{2} {3}", column.Name, column.Type,
                    string.IsNullOrWhiteSpace(column.MaximumSize) ? string.Empty : string.Format("({0})", column.MaximumSize),
                    column.AllowsNull ? "NULL" : "NOT NULL"));
            }

            database.ExecuteNonQuery(string.Format(@"CREATE TABLE [{0}].[{1}]({2})",
                tableDescription.Schema, tableDescription.Name, string.Join(",", columns)));
        }

        public void Remove(string schema, string tableName) {
            var table = sqlServerDatabase.GetTable(schema, tableName);
            if (table == null) throw new TableNotFoundException();

            var primaryKey = sqlServerDatabase.GetPrimaryKey(new TableDescription {Schema = schema, Name = tableName});
            if (primaryKey != null) {
                var foreignKeys = sqlServerDatabase.GetForeignKeysReferencing(primaryKey);
                if (foreignKeys.Any()) throw new ReferencedTableException();
            }

            database.ExecuteNonQuery(string.Format(@"DROP TABLE [{0}].[{1}]", schema, tableName));
        }

        private bool TableNameIsValid(TableDescription tableDescription) {
            return !string.IsNullOrWhiteSpace(tableDescription.Name) &&
                   sqlServerDatabase.GetTable(tableDescription.Schema, tableDescription.Name) == null &&
                   sqlServerDatabase.IdentifierNameIsValid(tableDescription.Name);
        }
    }
}