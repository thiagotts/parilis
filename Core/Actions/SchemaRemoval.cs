namespace Core.Actions {
    internal class SchemaCreation : SchemaAction {
        public SchemaCreation(ConnectionInfo connectionInfo, string schemaName) : base(connectionInfo, schemaName) {}

        public override string Description {
            get { return string.Format("Creating schema {0}.", SchemaName); }
        }

        internal override void Execute() {
            Schemas.Create(SchemaName);
        }
    }
}