namespace Core.Actions {
    internal class SchemaRemoval : SchemaAction {
        public SchemaRemoval(ConnectionInfo connectionInfo, string schemaName) : base(connectionInfo, schemaName) { }

        public override string Description => $"Removing schema {SchemaName}.";

        internal override void Execute() {
            Schemas.Remove(SchemaName);
        }
    }
}