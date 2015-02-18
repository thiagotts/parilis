using Core.Descriptions;

namespace Core.Actions {
    internal class UniqueCreation : ConstraintAction {
        internal readonly UniqueDescription UniqueDescription;

        public UniqueCreation(ConnectionInfo connectionInfo, UniqueDescription uniqueDescription) : base(connectionInfo) {
            this.UniqueDescription = uniqueDescription;
        }

        internal override void Execute() {
            Constraints.CreateUnique(UniqueDescription);
        }
    }
}