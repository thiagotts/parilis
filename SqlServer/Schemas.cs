using System;
using Core.Exceptions;
using Core.Interfaces;
using Microsoft.SqlServer.Management.Smo;

namespace SqlServer {
    public class Schemas : ISchema {
        private readonly Database database;
        private readonly SqlServerDatabase sqlServerDatabase;

        public Schemas(Database database) {
            this.database = database;
            sqlServerDatabase = new SqlServerDatabase(database);
        }

        public void Create(string schemaName) {
            if (sqlServerDatabase.SchemaExists(schemaName) || !sqlServerDatabase.IdentifierNameIsValid(schemaName))
                throw new InvalidSchemaNameException();

            database.ExecuteNonQuery(string.Format(@"CREATE SCHEMA {0}", schemaName));
        }

        public void Remove(string schemaName) {
            throw new NotImplementedException();
        }
    }
}