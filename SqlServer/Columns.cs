using System;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Descriptions;
using Core.Exceptions;
using Core.Interfaces;
using Microsoft.SqlServer.Management.Smo;
using SqlServer.Enums;
using DataType = SqlServer.Enums.DataType;

namespace SqlServer {
    public class Columns : IColumn {
        private readonly Database database;
        private readonly SqlServerDatabase sqlServerDatabase;

        public Columns(Database database) {
            this.database = database;
            sqlServerDatabase = new SqlServerDatabase(database);
        }

        public void Create(ColumnDescription column) {
            if (sqlServerDatabase.GetTable(column.Schema, column.TableName) == null)
                throw new TableNotFoundException();

            if (sqlServerDatabase.GetColumn(column.Schema, column.TableName, column.Name) != null)
                throw new InvalidColumnNameException();

            if (!ColumnNameIsValid(column.Name))
                throw new InvalidColumnNameException();

            if (!sqlServerDatabase.DataTypeIsValid(column.Type))
                throw new InvalidDataTypeException();

            if (!MaximumSizeIsValid(column))
                throw new InvalidDataTypeException();

            database.ExecuteNonQuery(string.Format(@"ALTER TABLE {0}.{1} ADD {2} {3}{4} {5}",
                column.Schema, column.TableName, column.Name, column.Type,
                string.IsNullOrWhiteSpace(column.MaximumSize) ? string.Empty : string.Format("({0})", column.MaximumSize),
                column.AllowsNull ? "NULL" : "NOT NULL"));
        }

        public void Remove(ColumnDescription column) {
            throw new NotImplementedException();
        }

        public void ChangeType(ColumnDescription @from, ColumnDescription to) {
            throw new NotImplementedException();
        }

        private bool ColumnNameIsValid(string columnName) {
            if (string.IsNullOrWhiteSpace(columnName)) return false;

            var firstCharacter = columnName[0].ToString();
            if (!Regex.IsMatch(firstCharacter, @"[a-zA-Z_@#]")) return false;

            if (!Regex.IsMatch(columnName, @"^[a-zA-Z0-9_@#$]*$")) return false;

            var keywords = Enums.Enums.GetDescriptions<Keyword>();
            if (keywords.Any(k => k.Equals(columnName, StringComparison.InvariantCultureIgnoreCase))) return false;

            return true;
        }

        private bool MaximumSizeIsValid(ColumnDescription column) {
            if (string.IsNullOrWhiteSpace(column.MaximumSize)) return true;

            if (!column.Type.Equals("varchar", StringComparison.InvariantCultureIgnoreCase) &&
                !column.Type.Equals("nvarchar", StringComparison.InvariantCultureIgnoreCase))
                return false;

            if (column.MaximumSize.Equals("max", StringComparison.InvariantCultureIgnoreCase))
                return true;

            int maximumSize;
            bool parsedSuccessfully = Int32.TryParse(column.MaximumSize, out maximumSize);
            if (!parsedSuccessfully || maximumSize <= 0) return false;

            if (column.Type.Equals("varchar", StringComparison.InvariantCultureIgnoreCase)) {
                return maximumSize < 8001;
            }
            else if (column.Type.Equals("nvarchar", StringComparison.InvariantCultureIgnoreCase)) {
                return maximumSize < 4001;
            }

            return true;
        }
    }
}