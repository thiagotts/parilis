using Core.Descriptions;
using Core.Interfaces;

namespace Core.Actions {
    internal abstract class IndexAction : Action {
        internal readonly IIndex Indexes;
        protected readonly IndexDescription IndexDescription;


        protected IndexAction(ConnectionInfo connectionInfo, IndexDescription indexDescription) : base(connectionInfo) {
            this.IndexDescription = indexDescription;
            Indexes = Components.Instance.GetComponent<IIndex>(connectionInfo);
        }
    }
}