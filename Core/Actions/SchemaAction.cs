using Core.Interfaces;

namespace Core.Actions {
    internal abstract class SchemaAction : Action {
        internal readonly ISchema Schemas;
        internal readonly string SchemaName;

        protected SchemaAction(ConnectionInfo connectionInfo, string schemaName) {
            SchemaName = schemaName;
            Schemas = Components.Instance.GetComponent<ISchema>(connectionInfo);
        }
    }
}