using Core.Descriptions;

namespace Core.Actions {
    internal class IndexCreation : IndexAction {
        public IndexCreation(ConnectionInfo connectionInfo, IndexDescription columnDescription) : base(connectionInfo, columnDescription) {}

        internal override void Execute() {
            Indexes.Create(ColumnDescription);
        }
    }
}