using Core.Descriptions;

namespace Core.Actions {
    internal class TableRemoval : TableAction {
        public TableRemoval(ConnectionInfo connectionInfo, TableDescription tableDescription) : base(connectionInfo, tableDescription) { }

        internal override void Execute() {
            Tables.Remove(TableDescription.Schema, TableDescription.Name);
        }
    }
}