using System;
using System.Linq;
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

        public void RemovePrimaryKey(PrimaryKeyDescription primaryKeyDescription) {
            var sqlServerDatabase = new SqlServerDatabase(database);
            var primaryKey = sqlServerDatabase.GetPrimaryKey(primaryKeyDescription.Name, primaryKeyDescription.Schema);
            if (primaryKey == null) throw new ConstraintNotFoundException();

            var foreignKeys = sqlServerDatabase.GetForeignKeysReferencing(primaryKeyDescription);
            if(foreignKeys.Any()) throw new ReferencedConstraintException();

            database.ExecuteNonQuery(string.Format(@"ALTER TABLE {0}.{1} DROP CONSTRAINT {2}",
                primaryKeyDescription.Schema, primaryKeyDescription.TableName, primaryKeyDescription.Name));
        }

        public void CreateForeignKey(ForeignKeyDescription foreignKeyDescription) {
            var sqlServerDatabase = new SqlServerDatabase(database);
            var foreignKeys = sqlServerDatabase.GetForeignKeys(new TableDescription {Schema = foreignKeyDescription.Schema, Name = foreignKeyDescription.TableName});
            
            if(foreignKeys.Any(key => key.Name.Equals(foreignKeyDescription.Name, StringComparison.InvariantCultureIgnoreCase)))
                throw new InvalidConstraintNameException();

            var primaryKey = sqlServerDatabase.GetPrimaryKey(new TableDescription {
                Schema = foreignKeyDescription.Columns.Values.First().Schema,
                Name = foreignKeyDescription.Columns.Values.First().TableName
            });

            if (!primaryKey.ColumnNames.Any(c => c.Equals(foreignKeyDescription.Columns.Values.First().Name, StringComparison.InvariantCultureIgnoreCase)))
                throw new InvalidReferenceColumnException();

            database.ExecuteNonQuery(string.Format(@"ALTER TABLE {0}.{1} ADD CONSTRAINT {2} FOREIGN KEY ({3}) REFERENCES {4}.{5}({6})",
                foreignKeyDescription.Schema, foreignKeyDescription.TableName, foreignKeyDescription.Name, foreignKeyDescription.Columns.Values.First().Name,
                foreignKeyDescription.Columns.Values.First().Schema, foreignKeyDescription.Columns.Values.First().TableName, foreignKeyDescription.Columns.Values.First().Name));
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