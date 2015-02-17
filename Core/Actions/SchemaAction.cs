using Core.Interfaces;

namespace Core.Actions {
    internal abstract class SchemaAction : Action {
        internal readonly ISchema Schemas;
        protected readonly string SchemaName;

        protected SchemaAction(ConnectionInfo connectionInfo, string schemaName) : base(connectionInfo) {
            SchemaName = schemaName;
            Schemas = Components.Instance.GetComponent<ISchema>(connectionInfo);
        }
    }
}