using Core.Descriptions;

namespace Core.Actions {
    internal class UniqueCreation : ConstraintAction {
        internal readonly UniqueDescription UniqueDescription;

        public UniqueCreation(ConnectionInfo connectionInfo, UniqueDescription uniqueDescription) : base(connectionInfo) {
            this.UniqueDescription = uniqueDescription;
        }

        public override string Description => $"Creating unique key {UniqueDescription.FullName} in table {UniqueDescription.TableName}.";

        internal override void Execute() {
            Constraints.CreateUnique(UniqueDescription);
        }
    }
}