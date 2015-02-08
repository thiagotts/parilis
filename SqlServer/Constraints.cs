using System;
using System.Linq;
using Core.Descriptions;
using Core.Exceptions;
using Core.Interfaces;
using Microsoft.SqlServer.Management.Smo;

namespace SqlServer {
    public class Constraints : IConstraint {
        private readonly Database database;
        private readonly SqlServerDatabase sqlServerDatabase;

        public Constraints(Database database) {
            this.database = database;
            sqlServerDatabase = new SqlServerDatabase(database);
        }

        public void CreatePrimaryKey(PrimaryKeyDescription primaryKeyDescription) {
            var primaryKey = sqlServerDatabase.GetPrimaryKey(new TableDescription {
                Schema = primaryKeyDescription.Schema,
                Name = primaryKeyDescription.TableName
            });

            if (primaryKey != null) throw new MultiplePrimaryKeysException();

            primaryKey = sqlServerDatabase.GetPrimaryKey(primaryKeyDescription.Name, primaryKeyDescription.Schema);
            if (primaryKey != null) throw new InvalidConstraintNameException();

            database.ExecuteNonQuery(string.Format(@"
                ALTER TABLE {0}
                ADD CONSTRAINT {1} PRIMARY KEY ({2})",
                primaryKeyDescription.TableName, primaryKeyDescription.Name, string.Join(",", primaryKeyDescription.ColumnNames)));
        }

        public void RemovePrimaryKey(PrimaryKeyDescription primaryKeyDescription) {
            var primaryKey = sqlServerDatabase.GetPrimaryKey(primaryKeyDescription.Name, primaryKeyDescription.Schema);
            if (primaryKey == null) throw new ConstraintNotFoundException();

            var foreignKeys = sqlServerDatabase.GetForeignKeysReferencing(primaryKeyDescription);
            if (foreignKeys.Any()) throw new ReferencedConstraintException();

            database.ExecuteNonQuery(string.Format(@"ALTER TABLE {0}.{1} DROP CONSTRAINT {2}",
                primaryKeyDescription.Schema, primaryKeyDescription.TableName, primaryKeyDescription.Name));
        }

        public void CreateForeignKey(ForeignKeyDescription foreignKeyDescription) {
            if (!DescriptionIsValid(foreignKeyDescription))
                throw new InvalidDescriptionException();

            if (ThereIsAnotherForeignKeyWithTheSameName(foreignKeyDescription))
                throw new InvalidConstraintNameException();

            if (!ReferencedColumnsAreValid(foreignKeyDescription))
                throw new InvalidReferenceColumnException();

            database.ExecuteNonQuery(string.Format(@"ALTER TABLE {0}.{1} ADD CONSTRAINT {2} FOREIGN KEY ({3}) REFERENCES {4}.{5}({6})",
                foreignKeyDescription.Schema, foreignKeyDescription.TableName, foreignKeyDescription.Name,
                string.Join(",", foreignKeyDescription.Columns.Keys), foreignKeyDescription.Columns.Values.First().Schema,
                foreignKeyDescription.Columns.Values.First().TableName, string.Join(",", foreignKeyDescription.Columns.Values.Select(v => v.Name))));
        }

        public void RemoveForeignKey(ConstraintDescription foreignKeyDescription) {
            var foreignKeys = sqlServerDatabase.GetForeignKeys(new TableDescription {
                Schema = foreignKeyDescription.Schema,
                Name = foreignKeyDescription.TableName
            });

            if(!foreignKeys.Any(key => key.Schema.Equals(foreignKeyDescription.Schema) &&
                key.TableName.Equals(foreignKeyDescription.TableName) &&
                key.Name.Equals(foreignKeyDescription.Name)))
                throw new ConstraintNotFoundException();

            database.ExecuteNonQuery(string.Format(@"ALTER TABLE {0}.{1} DROP CONSTRAINT {2}",
                foreignKeyDescription.Schema, foreignKeyDescription.TableName, foreignKeyDescription.Name));
        }

        public void CreateUnique(UniqueDescription uniqueDescription) {
            var uniqueKey = sqlServerDatabase.GetUniqueKey(uniqueDescription.Name);
            if (uniqueKey != null) throw new InvalidConstraintNameException();

            database.ExecuteNonQuery(string.Format(@"ALTER TABLE {0}.{1} ADD CONSTRAINT {2} UNIQUE ({3})",
                uniqueDescription.Schema, uniqueDescription.TableName, uniqueDescription.Name,
                string.Join(",", uniqueDescription.ColumnNames)));
        }

        public void RemoveUnique(UniqueDescription uniqueDescription) {
            throw new NotImplementedException();
        }

        public void CreateDefault(DefaultDescription defaultDescription) {
            throw new NotImplementedException();
        }

        public void RemoveDefault(ConstraintDescription defaultDescription) {
            throw new NotImplementedException();
        }

        private bool DescriptionIsValid(ForeignKeyDescription foreignKeyDescription) {
            foreach (var column in foreignKeyDescription.Columns.Values) {
                if (foreignKeyDescription.Columns.Values.Count(v =>
                    v.Name.Equals(column.Name, StringComparison.InvariantCultureIgnoreCase) &&
                    v.TableName.Equals(column.TableName, StringComparison.InvariantCultureIgnoreCase)) > 1) {
                    return false;
                }
            }

            return true;
        }

        private bool ThereIsAnotherForeignKeyWithTheSameName(ForeignKeyDescription foreignKeyDescription) {
            var foreignKeys = sqlServerDatabase.GetForeignKeys(new TableDescription {
                Schema = foreignKeyDescription.Schema,
                Name = foreignKeyDescription.TableName
            });

            return foreignKeys.Any(key => key.Name.Equals(foreignKeyDescription.Name, StringComparison.InvariantCultureIgnoreCase));
        }

        private bool ReferencedColumnsAreValid(ForeignKeyDescription foreignKeyDescription) {
            foreach (var column in foreignKeyDescription.Columns.Values) {
                var primaryKey = sqlServerDatabase.GetPrimaryKey(new TableDescription {
                    Schema = column.Schema,
                    Name = column.TableName
                });

                if (primaryKey == null || !primaryKey.ColumnNames.Any(c => c.Equals(column.Name, StringComparison.InvariantCultureIgnoreCase)))
                    return false;
            }

            return true;
        }
    }
}