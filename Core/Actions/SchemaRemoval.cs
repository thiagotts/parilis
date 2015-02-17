namespace Core.Actions {
    internal class SchemaCreation : SchemaAction {
        public SchemaCreation(ConnectionInfo connectionInfo, string schemaName) : base(connectionInfo, schemaName) {}

        internal override void Execute() {
            Schemas.Create(SchemaName);
        }
    }
}