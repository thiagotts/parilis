using Core.Descriptions;
using Core.Interfaces;

namespace Core.Actions {
    internal abstract class IndexAction : Action {
        internal readonly IIndex Indexes;
        internal readonly IndexDescription IndexDescription;

        protected IndexAction(ConnectionInfo connectionInfo, IndexDescription indexDescription) {
            IndexDescription = indexDescription;
            Indexes = Components.Instance.GetComponent<IIndex>(connectionInfo);
        }
    }
}