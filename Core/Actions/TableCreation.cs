using Core.Descriptions;

namespace Core.Actions {
    internal class TableCreation : TableAction {
        public TableCreation(ConnectionInfo connectionInfo, TableDescription tableDescription) : base(connectionInfo, tableDescription) { }

        public override string Description => $"Creating table {TableDescription.FullName}.";

        internal override void Execute() {
            Tables.Create(TableDescription);
        }
    }
}