using Core.Descriptions;

namespace Core.Actions {
    internal class ColumnModification : ColumnAction {
        public ColumnModification(ConnectionInfo connectionInfo, ColumnDescription columnDescription) : base(connectionInfo, columnDescription) { }

        internal override void Execute() {
            Columns.ChangeType(ColumnDescription);
        }
    }
}