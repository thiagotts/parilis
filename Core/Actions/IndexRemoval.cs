using Core.Descriptions;

namespace Core.Actions {
    internal class IndexRemoval : IndexAction {
        public IndexRemoval(ConnectionInfo connectionInfo, IndexDescription indexDescription) : base(connectionInfo, indexDescription) { }

        public override string Description => $"Removing index {IndexDescription.FullName}.";

        internal override void Execute() {
            Indexes.Remove(IndexDescription);
        }
    }
}