using Core.Descriptions;

namespace Core.Actions {
    internal class IndexRemoval : IndexAction {
        public IndexRemoval(ConnectionInfo connectionInfo, IndexDescription indexDescription) : base(connectionInfo, indexDescription) { }

        internal override void Execute() {
            Indexes.Remove(IndexDescription);
        }
    }
}