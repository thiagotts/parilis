using Core.Descriptions;

namespace Core.Actions {
    internal class PrimaryKeyRemoval : ConstraintAction {
        internal readonly PrimaryKeyDescription PrimaryKeyDescription;

        public PrimaryKeyRemoval(ConnectionInfo connectionInfo, PrimaryKeyDescription primaryKeyDescription) : base(connectionInfo) {
            this.PrimaryKeyDescription = primaryKeyDescription;
        }

        internal override void Execute() {
            Constraints.RemovePrimaryKey(PrimaryKeyDescription);
        }
    }
}