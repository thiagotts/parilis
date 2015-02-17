using Core.Descriptions;

namespace Core.Actions {
    internal class PrimaryKeyRemoval : ConstraintAction {
        private readonly PrimaryKeyDescription primaryKeyDescription;

        public PrimaryKeyRemoval(ConnectionInfo connectionInfo, PrimaryKeyDescription primaryKeyDescription) : base(connectionInfo) {
            this.primaryKeyDescription = primaryKeyDescription;
        }

        internal override void Execute() {
            Constraints.RemovePrimaryKey(primaryKeyDescription);
        }
    }
}