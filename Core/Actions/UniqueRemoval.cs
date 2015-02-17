using Core.Descriptions;

namespace Core.Actions {
    internal class UniqueRemoval : ConstraintAction {
        private readonly UniqueDescription uniqueDescription;

        public UniqueRemoval(ConnectionInfo connectionInfo, UniqueDescription uniqueDescription) : base(connectionInfo) {
            this.uniqueDescription = uniqueDescription;
        }

        internal override void Execute() {
            Constraints.RemoveUnique(uniqueDescription);
        }
    }
}