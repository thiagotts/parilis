using Core.Descriptions;

namespace Core.Actions {
    internal class TableCreation : TableAction {
        public TableCreation(ConnectionInfo connectionInfo, TableDescription tableDescription) : base(connectionInfo, tableDescription) { }

        internal override void Execute() {
            Tables.Create(TableDescription);
        }
    }
}