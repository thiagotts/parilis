using Core.Descriptions;

namespace Core.Actions {
    internal class IndexRemoval : IndexAction {
        public IndexRemoval(ConnectionInfo connectionInfo, IndexDescription indexDescription) : base(connectionInfo, indexDescription) { }

        public override string Description {
            get { return string.Format("Removing index {0}.", IndexDescription.FullName); }
        }

        internal override void Execute() {
            Indexes.Remove(IndexDescription);
        }
    }
}