using Core.Descriptions;
using Core.Interfaces;

namespace Core.Actions {
    internal abstract class TableAction : Action {
        internal readonly ITable Tables;
        protected readonly TableDescription TableDescription;

        protected TableAction(ConnectionInfo connectionInfo, TableDescription tableDescription) : base(connectionInfo) {
            TableDescription = tableDescription;
            Tables = Components.Instance.GetComponent<ITable>(connectionInfo);
        }
    }
}