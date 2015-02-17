using Core.Descriptions;

namespace Core.Actions {
    internal class PrimaryKeyCreation : ConstraintAction {
        private readonly PrimaryKeyDescription primaryKeyDescription;

        public PrimaryKeyCreation(ConnectionInfo connectionInfo, PrimaryKeyDescription primaryKeyDescription) : base(connectionInfo) {
            this.primaryKeyDescription = primaryKeyDescription;
        }

        internal override void Execute() {
            Constraints.CreatePrimaryKey(primaryKeyDescription);
        }
    }
}