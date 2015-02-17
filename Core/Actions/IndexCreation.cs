using Core.Descriptions;

namespace Core.Actions {
    internal class IndexCreation : IndexAction {
        public IndexCreation(ConnectionInfo connectionInfo, IndexDescription tableDescription) : base(connectionInfo, tableDescription) {}

        internal override void Execute() {
            Indexes.Create(TableDescription);
        }
    }
}