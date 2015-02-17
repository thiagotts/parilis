using Core.Descriptions;

namespace Core.Actions {
    internal class UniqueCreation : ConstraintAction {
        private readonly UniqueDescription uniqueDescription;

        public UniqueCreation(ConnectionInfo connectionInfo, UniqueDescription uniqueDescription) : base(connectionInfo) {
            this.uniqueDescription = uniqueDescription;
        }

        internal override void Execute() {
            Constraints.CreateUnique(uniqueDescription);
        }
    }
}