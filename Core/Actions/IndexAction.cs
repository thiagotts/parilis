using Core.Descriptions;
using Core.Interfaces;

namespace Core.Actions {
    internal abstract class IndexAction : Action {
        internal readonly IIndex Indexes;
        protected readonly IndexDescription TableDescription;


        protected IndexAction(ConnectionInfo connectionInfo, IndexDescription tableDescription) : base(connectionInfo) {
            this.TableDescription = tableDescription;
            Indexes = Components.Instance.GetComponent<IIndex>(connectionInfo);
        }
    }
}