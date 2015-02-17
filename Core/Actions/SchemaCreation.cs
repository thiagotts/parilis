namespace Core.Actions {
    internal class SchemaRemoval : SchemaAction {
        public SchemaRemoval(ConnectionInfo connectionInfo, string schemaName) : base(connectionInfo, schemaName) { }

        internal override void Execute() {
            Schemas.Remove(SchemaName);
        }
    }
}