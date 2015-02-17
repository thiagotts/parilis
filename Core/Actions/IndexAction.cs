using Core.Descriptions;
using Core.Interfaces;

namespace Core.Actions {
    internal abstract class IndexAction : Action {
        internal readonly IIndex Indexes;
        protected readonly IndexDescription ColumnDescription;


        protected IndexAction(ConnectionInfo connectionInfo, IndexDescription columnDescription) : base(connectionInfo) {
            this.ColumnDescription = columnDescription;
            Indexes = Components.Instance.GetComponent<IIndex>(connectionInfo);
        }
    }
}