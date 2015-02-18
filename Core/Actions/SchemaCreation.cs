namespace Core.Actions {
    internal class SchemaRemoval : SchemaAction {
        public SchemaRemoval(ConnectionInfo connectionInfo, string schemaName) : base(connectionInfo, schemaName) { }

        public override string Description {
            get { return string.Format("Removing schema {0}.", SchemaName); }
        }

        internal override void Execute() {
            Schemas.Remove(SchemaName);
        }
    }
}