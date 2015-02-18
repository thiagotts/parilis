using Core.Descriptions;

namespace Core.Actions {
    internal class IndexCreation : IndexAction {
        public IndexCreation(ConnectionInfo connectionInfo, IndexDescription indexDescription) : base(connectionInfo, indexDescription) {}

        internal override void Execute() {
            Indexes.Create(IndexDescription);
        }
    }
}