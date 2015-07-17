using Core.Descriptions;
using Core.Interfaces;

namespace Core.Actions {
    internal abstract class ColumnAction : Action {
        internal readonly IDatabase Database;
        internal readonly IColumn Columns;
        internal readonly IConstraint Constraints;
        internal readonly ColumnDescription ColumnDescription;

        protected ColumnAction(ConnectionInfo connectionInfo, ColumnDescription columnDescription) {
            Database = Components.Instance.GetComponent<IDatabase>(connectionInfo);
            ColumnDescription = columnDescription;
            Columns = Components.Instance.GetComponent<IColumn>(connectionInfo);
            Constraints = Components.Instance.GetComponent<IConstraint>(connectionInfo);
        }
    }
}