using Core.Descriptions;

namespace Core.Actions {
    internal class IndexRemoval : IndexAction {
        public IndexRemoval(ConnectionInfo connectionInfo, IndexDescription columnDescription) : base(connectionInfo, columnDescription) { }

        internal override void Execute() {
            Indexes.Remove(ColumnDescription);
        }
    }
}