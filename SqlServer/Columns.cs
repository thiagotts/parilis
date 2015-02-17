using System;
using System.Data.SqlClient;
using System.Linq;
using Castle.Core;
using Core;
using Core.Descriptions;
using Core.Exceptions;
using Core.Interfaces;
using Microsoft.SqlServer.Management.Smo;

namespace SqlServer {
    [CastleComponent("SqlServer.Columns", typeof (IColumn), Lifestyle = LifestyleType.Transient)]
    public class Columns : SqlServerEntity, IColumn {
        public Columns(ConnectionInfo database) {
            Initialize(database);
        }

        public void Create(ColumnDescription column) {
            if (SqlServerDatabase.GetTable(column.Schema, column.TableName) == null)
                throw new TableNotFoundException();

            if (SqlServerDatabase.GetColumn(column.Schema, column.TableName, column.Name) != null)
                throw new InvalidColumnNameException();

            if (!SqlServerDatabase.IdentifierNameIsValid(column.Name))
                throw new InvalidColumnNameException();

            if (!SqlServerDatabase.DataTypeIsValid(column.Type))
                throw new InvalidDataTypeException();

            if (!MaximumSizeIsValid(column))
                throw new InvalidDataTypeException();

            Database.ExecuteNonQuery(string.Format(@"ALTER TABLE {0}.{1} ADD {2} {3}{4} {5}",
                column.Schema, column.TableName, column.Name, column.Type,
                string.IsNullOrWhiteSpace(column.MaximumSize) ? string.Empty : string.Format("({0})", column.MaximumSize),
                column.AllowsNull ? "NULL" : "NOT NULL"));
        }

        public void Remove(ColumnDescription column) {
            var table = SqlServerDatabase.GetTable(column.Schema, column.TableName);
            if (table == null) throw new TableNotFoundException();
            if (table.Columns.Count == 1) throw new SingleColumnException();

            if (!table.Columns.Any(c => c.Name.Equals(column.Name, StringComparison.InvariantCultureIgnoreCase)))
                throw new ColumnNotFoundException();

            if (ColumnIsReferencedByAConstraint(column))
                throw new ReferencedColumnException();

            Database.ExecuteNonQuery(string.Format(@"ALTER TABLE {0}.{1} DROP COLUMN {2}",
                column.Schema, column.TableName, column.Name));
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

            if (!MaximumSizeIsValid(column))
                throw new InvalidDataTypeException();

            try {
                Database.ExecuteNonQuery(string.Format(@"ALTER TABLE {0}.{1} ALTER COLUMN {2} {3}{4} {5}",
                    column.Schema, column.TableName, column.Name, column.Type,
                    string.IsNullOrWhiteSpace(column.MaximumSize) ? string.Empty : string.Format("({0})", column.MaximumSize),
                    column.AllowsNull ? "NULL" : "NOT NULL"));
            }
            catch (FailedOperationException ex) {
                if (ex.InnerException != null &&
                    ex.InnerException.InnerException != null &&
                    ex.InnerException.InnerException is SqlException &&
                    (ex.InnerException.InnerException as SqlException).Number == 8114)
                    throw new InvalidDataTypeException("The existent values could not be converted to the new data type.");

                else throw;
            }
        }

        private bool MaximumSizeIsValid(ColumnDescription column) {
            if (string.IsNullOrWhiteSpace(column.MaximumSize)) return true;

            if (!column.Type.Equals("varchar", StringComparison.InvariantCultureIgnoreCase) &&
                !column.Type.Equals("nvarchar", StringComparison.InvariantCultureIgnoreCase))
                return false;

            if (column.MaximumSize.Equals("max", StringComparison.InvariantCultureIgnoreCase))
                return true;

            int maximumSize;
            var parsedSuccessfully = Int32.TryParse(column.MaximumSize, out maximumSize);
            if (!parsedSuccessfully || maximumSize <= 0) return false;

            if (column.Type.Equals("varchar", StringComparison.InvariantCultureIgnoreCase)) {
                return maximumSize < 8001;
            }
            else if (column.Type.Equals("nvarchar", StringComparison.InvariantCultureIgnoreCase)) {
                return maximumSize < 4001;
            }

            return true;
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
                   primaryKey.ColumnNames.Any(c => c.Equals(column.Name, StringComparison.InvariantCultureIgnoreCase));
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
                if (uniqueKey.ColumnNames.Any(c => c.Equals(column.Name, StringComparison.InvariantCultureIgnoreCase)))
                    return true;
            }

            return false;
        }

        private bool ColumnIsReferencedByADefault(ColumnDescription column) {
            var defaults = SqlServerDatabase.GetDefaults();
            foreach (var defaultDescription in defaults) {
                if (defaultDescription.Schema.Equals(column.Schema, StringComparison.InvariantCultureIgnoreCase) &&
                    defaultDescription.TableName.Equals(column.TableName, StringComparison.InvariantCultureIgnoreCase) &&
                    defaultDescription.ColumnName.Equals(column.Name, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        private bool ColumnIsReferencedByAnIndex(ColumnDescription column) {
            var indexes = SqlServerDatabase.GetIndexes(column.Schema, column.TableName);
            foreach (var index in indexes) {
                if (index.ColumnNames.Any(c => c.Equals(column.Name, StringComparison.InvariantCultureIgnoreCase)))
                    return true;
            }

            return false;
        }
    }
}