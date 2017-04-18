using Core.Descriptions;

namespace Core.Actions {
    internal class IndexCreation : IndexAction {
        public IndexCreation(ConnectionInfo connectionInfo, IndexDescription indexDescription) : base(connectionInfo, indexDescription) {}

        public override string Description => $"Creating index {IndexDescription.FullName}.";

        internal override void Execute() {
            Indexes.Create(IndexDescription);
        }
    }
}