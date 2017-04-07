using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Castle.Core;
using Core;
using Core.Descriptions;
using Core.Exceptions;
using Core.Interfaces;
using Microsoft.SqlServer.Management.Smo;
using SqlServer.Attributes;

namespace SqlServer {
    [CastleComponent("SqlServer.Columns", typeof (IColumn), Lifestyle = LifestyleType.Transient)]
    public class Columns : SqlServerEntity, IColumn {
        private IDictionary<string, AllowsLengthAttribute> dataTypeLengthProperties;

        public Columns(ConnectionInfo database) {
            Initialize(database);
            dataTypeLengthProperties = GetDataTypeLengthProperties();
        }

        public void Create(ColumnDescription column) {
            if (column.AllowsNull && column.IsIdentity)
                throw new InvalidDescriptionException();

            if (SqlServerDatabase.GetTable(column.Schema, column.TableName) == null)
                throw new TableNotFoundException();

            if (SqlServerDatabase.GetColumn(column.Schema, column.TableName, column.Name) != null)
                throw new InvalidColumnNameException();

            if (!SqlServerDatabase.IdentifierNameIsValid(column.Name))
                throw new InvalidColumnNameException();

            if (!SqlServerDatabase.DataTypeIsValid(column.Type))
                throw new InvalidDataTypeException();

            if (!LengthIsValid(column))
                throw new InvalidDataTypeException();

            var command = new SqlCommand(string.Format(@"ALTER TABLE [{0}].[{1}] ADD [{2}] {3}{4} {5} {6}",
                column.Schema, column.TableName, column.Name, column.Type,
                string.IsNullOrWhiteSpace(column.Length) ? string.Empty : string.Format("({0})", column.Length),
                column.IsIdentity ? "IDENTITY(1,1)" : string.Empty, column.AllowsNull ? "NULL" : "NOT NULL"));

            SqlServerDatabase.ExecuteNonQuery(command);
        }

        public void Remove(ColumnDescription column) {
            var table = SqlServerDatabase.GetTable(column.Schema, column.TableName);
            if (table == null) throw new TableNotFoundException();
            if (table.Columns.Count == 1) throw new SingleColumnException();

            if (!table.Columns.Any(c => c.Name.Equals(column.Name, StringComparison.InvariantCultureIgnoreCase)))
                throw new ColumnNotFoundException();

            if (ColumnIsReferencedByAConstraint(column))
                throw new ReferencedColumnException();

            var command = new SqlCommand($@"ALTER TABLE [{column.Schema}].[{column.TableName}] DROP COLUMN [{column.Name}]");

            SqlServerDatabase.ExecuteNonQuery(command);
        }

        public void ChangeType(ColumnDescription column) {
            var table = SqlServerDatabase.GetTable(column.Schema, column.TableName);
            if (table == null) throw new TableNotFoundException();

            if (!table.Columns.Any(c => c.Name.Equals(column.Name, StringComparison.InvariantCultureIgnoreCase)))
                throw new ColumnNotFoundException();

            if (!SqlServerDatabase.DataTypeIsValid(column.Type))
                throw new InvalidDataTypeException();

            if (ColumnIsReferencedByAConstraint(column))
                throw new ReferencedColumnException();

            if (!LengthIsValid(column))
                throw new InvalidDataTypeException();

            try {
                var command = new SqlCommand(string.Format(@"ALTER TABLE [{0}].[{1}] ALTER COLUMN [{2}] {3}{4} {5}",
                    column.Schema, column.TableName, column.Name, column.Type,
                    string.IsNullOrWhiteSpace(column.Length) ? string.Empty : string.Format("({0})", column.Length),
                    column.AllowsNull ? "NULL" : "NOT NULL"));

                SqlServerDatabase.ExecuteNonQuery(command);
            }
            catch (SqlException ex) {
                if (ex.Number == 8114)
                    throw new InvalidDataTypeException("The existent values could not be converted to the new data type.");

                else throw;
            }
        }

        private bool LengthIsValid(ColumnDescription column) {
            if (string.IsNullOrWhiteSpace(column.Length)) return true;

            if (!dataTypeLengthProperties[column.Type].AllowsLength)
                return false;

            if (column.Length.Equals("max", StringComparison.InvariantCultureIgnoreCase))
                return dataTypeLengthProperties[column.Type].AllowsMax;

            int maximumSize;
            var parsedSuccessfully = Int32.TryParse(column.Length, out maximumSize);
            if (!parsedSuccessfully) return false;

            return maximumSize <= dataTypeLengthProperties[column.Type].MaximumValue &&
                   maximumSize >= dataTypeLengthProperties[column.Type].MinimumValue;
        }

        private IDictionary<string, AllowsLengthAttribute> GetDataTypeLengthProperties() {
            Array values = Enum.GetValues(typeof (Enums.DataType));

            var result = new Dictionary<string, AllowsLengthAttribute>();
            foreach (var value in values) {
                var allowsLengthAttribute = Enums.Enums.GetAllowsLength(value);
                result.Add(Enums.Enums.GetDefaultValue(value), allowsLengthAttribute);
            }

            return result;
        }

        private bool ColumnIsReferencedByAConstraint(ColumnDescription column) {
            var tableDescription = new TableDescription {
                Schema = column.Schema,
                Name = column.TableName
            };

            return ColumnIsReferencedByAPrimaryKey(tableDescription, column) ||
                   ColumnIsReferencedByAForeignKey(tableDescription, column) ||
                   ColumnIsReferencedByAUniqueKey(tableDescription, column) ||
                   ColumnIsReferencedByADefault(column) ||
                   ColumnIsReferencedByAnIndex(column);
        }

        private bool ColumnIsReferencedByAPrimaryKey(TableDescription tableDescription, ColumnDescription column) {
            var primaryKey = SqlServerDatabase.GetPrimaryKey(tableDescription);
            return primaryKey != null &&
                   primaryKey.Columns.Any(c => c.Name.Equals(column.Name, StringComparison.InvariantCultureIgnoreCase));
        }

        private bool ColumnIsReferencedByAForeignKey(TableDescription tableDescription, ColumnDescription column) {
            var foreignKeys = SqlServerDatabase.GetForeignKeys(tableDescription);
            foreach (var foreignKey in foreignKeys) {
                if (foreignKey.Columns.Any(c => c.Key.Equals(column.Name, StringComparison.InvariantCultureIgnoreCase)))
                    return true;
            }

            return false;
        }

        private bool ColumnIsReferencedByAUniqueKey(TableDescription tableDescription, ColumnDescription column) {
            var uniqueKeys = SqlServerDatabase.GetUniqueKeys(tableDescription);
            foreach (var uniqueKey in uniqueKeys) {
                if (uniqueKey.Columns.Any(c => c.Name.Equals(column.Name, StringComparison.InvariantCultureIgnoreCase)))
                    return true;
            }

            return false;
        }

        private bool ColumnIsReferencedByADefault(ColumnDescription column) {
            var defaults = SqlServerDatabase.GetDefaults();
            foreach (var defaultDescription in defaults) {
                if (defaultDescription.Schema.Equals(column.Schema, StringComparison.InvariantCultureIgnoreCase) &&
                    defaultDescription.TableName.Equals(column.TableName, StringComparison.InvariantCultureIgnoreCase) &&
                    defaultDescription.Column.FullName.Equals(column.FullName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        private bool ColumnIsReferencedByAnIndex(ColumnDescription column) {
            var indexes = SqlServerDatabase.GetIndexes(column.Schema, column.TableName);
            foreach (var index in indexes) {
                if (index.Columns.Any(c => c.FullName.Equals(column.FullName, StringComparison.InvariantCultureIgnoreCase)))
                    return true;
            }

            return false;
        }
    }
}