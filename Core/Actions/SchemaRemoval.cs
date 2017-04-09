namespace Core.Actions {
    internal class SchemaCreation : SchemaAction {
        public SchemaCreation(ConnectionInfo connectionInfo, string schemaName) : base(connectionInfo, schemaName) {}

        public override string Description => $"Creating schema {SchemaName}.";

        internal override void Execute() {
            Schemas.Create(SchemaName);
        }
    }
}