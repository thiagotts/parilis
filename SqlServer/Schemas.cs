using System.Data.SqlClient;
using System.Linq;
using Castle.Core;
using Core;
using Core.Exceptions;
using Core.Interfaces;

namespace SqlServer {
    [CastleComponent("SqlServer.Schemas", typeof (ISchema), Lifestyle = LifestyleType.Transient)]
    public class Schemas : SqlServerEntity, ISchema {
        public Schemas(ConnectionInfo database) {
            Initialize(database);
        }

        public void Create(string schemaName) {
            if (SqlServerDatabase.SchemaExists(schemaName) || !SqlServerDatabase.IdentifierNameIsValid(schemaName))
                throw new InvalidSchemaNameException();

            var command = new SqlCommand(string.Format(@"CREATE SCHEMA [{0}]", schemaName));
            
            SqlServerDatabase.ExecuteNonQuery(command);
        }

        public void Remove(string schemaName) {
            if (!SqlServerDatabase.SchemaExists(schemaName))
                throw new SchemaNotFoundException();

            if (SchemaIsReferenced(schemaName))
                throw new ReferencedSchemaException();

            var command = new SqlCommand(string.Format(@"DROP SCHEMA [{0}]", schemaName));

            SqlServerDatabase.ExecuteNonQuery(command);
        }

        private bool SchemaIsReferenced(string schemaName) {
            Database.Schemas.Refresh();
            var schema = Database.Schemas[schemaName];
            if (schema == null) return false;

            var ownedObjects = schema.EnumOwnedObjects();
            return ownedObjects != null && ownedObjects.Any();
        }
    }
}