using Core.Interfaces;

namespace Core.Actions {
    internal abstract class ConstraintAction : Action {
        internal readonly IDatabase Database;
        internal readonly IConstraint Constraints;

        protected ConstraintAction(ConnectionInfo connectionInfo) {
            Database = Components.Instance.GetComponent<IDatabase>(connectionInfo);
            Constraints = Components.Instance.GetComponent<IConstraint>(connectionInfo);
        }
    }
}