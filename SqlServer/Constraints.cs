using System;
using Core.Descriptions;
using Core.Interfaces;
using Microsoft.SqlServer.Management.Smo;

namespace SqlServer {
    public class Constraints : IConstraint {
        private Database database;

        public Constraints(Database database) {
            this.database = database;
        }

        public void CreatePrimaryKey(PrimaryKeyDescription primaryKeyDescription) {
            throw new NotImplementedException();
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