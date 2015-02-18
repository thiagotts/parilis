using Core.Descriptions;

namespace Core.Actions {
    internal class UniqueRemoval : ConstraintAction {
        internal readonly UniqueDescription UniqueDescription;

        public UniqueRemoval(ConnectionInfo connectionInfo, UniqueDescription uniqueDescription) : base(connectionInfo) {
            this.UniqueDescription = uniqueDescription;
        }

        internal override void Execute() {
            Constraints.RemoveUnique(UniqueDescription);
        }
    }
}