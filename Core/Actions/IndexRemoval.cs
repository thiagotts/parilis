using Core.Descriptions;

namespace Core.Actions {
    internal class IndexRemoval : IndexAction {
        public IndexRemoval(ConnectionInfo connectionInfo, IndexDescription tableDescription) : base(connectionInfo, tableDescription) { }

        internal override void Execute() {
            Indexes.Remove(TableDescription);
        }
    }
}