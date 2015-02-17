using Core.Descriptions;

namespace Core.Actions {
    internal class ColumnCreation : ColumnAction {
        public ColumnCreation(ConnectionInfo connectionInfo, ColumnDescription columnDescription) : base(connectionInfo, columnDescription) { }

        internal override void Execute() {
            Columns.Create(ColumnDescription);
        }
    }
}