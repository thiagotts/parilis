using Core.Descriptions;

namespace Core.Actions {
    internal class PrimaryKeyCreation : ConstraintAction {
        internal readonly PrimaryKeyDescription PrimaryKeyDescription;

        public PrimaryKeyCreation(ConnectionInfo connectionInfo, PrimaryKeyDescription primaryKeyDescription) : base(connectionInfo) {
            this.PrimaryKeyDescription = primaryKeyDescription;
        }

        public override string Description => $"Creating primary key {PrimaryKeyDescription.FullName} in table {PrimaryKeyDescription.TableName}.";

        internal override void Execute() {
            Constraints.CreatePrimaryKey(PrimaryKeyDescription);
        }
    }
}