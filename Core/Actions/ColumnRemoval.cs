using Core.Descriptions;

namespace Core.Actions {
    internal class ColumnRemoval : ColumnAction {
        public ColumnRemoval(ConnectionInfo connectionInfo, ColumnDescription columnDescription) : base(connectionInfo, columnDescription) { }

        internal override void Execute() {
            Columns.Remove(ColumnDescription);
        }
    }
}