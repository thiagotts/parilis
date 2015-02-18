using Core.Descriptions;
using Core.Interfaces;

namespace Core.Actions {
    internal abstract class TableAction : Action {
        internal readonly ITable Tables;
        internal readonly TableDescription TableDescription;

        protected TableAction(ConnectionInfo connectionInfo, TableDescription tableDescription) {
            TableDescription = tableDescription;
            Tables = Components.Instance.GetComponent<ITable>(connectionInfo);
        }
    }
}