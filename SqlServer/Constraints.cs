using System;
using Core.Descriptions;
using Core.Exceptions;
using Core.Interfaces;
using Microsoft.SqlServer.Management.Smo;

namespace SqlServer {
    public class Constraints : IConstraint {
        private Database database;

        public Constraints(Database database) {
            this.database = database;
        }

        public void CreatePrimaryKey(PrimaryKeyDescription primaryKeyDescription) {
            var sqlServerDatabase = new SqlServerDatabase(database);
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

        public void RemovePrimaryKey(ConstraintDescription primaryKeyDescription) {
            throw new NotImplementedException();
        }

        public void CreateForeignKey(ForeignKeyDescription foreignKeyDescription) {
            throw new NotImplementedException();
        }

        public void RemoveForeignKey(ConstraintDescription foreignKeyDescription) {
            throw new NotImplementedException();
        }

        public void CreateUnique(UniqueDescription uniqueDescription) {
            throw new NotImplementedException();
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
    }
}