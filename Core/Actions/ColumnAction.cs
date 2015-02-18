using Core.Descriptions;
using Core.Interfaces;

namespace Core.Actions {
    internal abstract class ColumnAction : Action {
        internal readonly IColumn Columns;
        internal readonly ColumnDescription ColumnDescription;

        protected ColumnAction(ConnectionInfo connectionInfo, ColumnDescription columnDescription) : base(connectionInfo) {
            ColumnDescription = columnDescription;
            Columns = Components.Instance.GetComponent<IColumn>(connectionInfo);
        }
    }
}