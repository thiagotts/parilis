using Core.Descriptions;

namespace Core.Actions {
    internal class UniqueRemoval : ConstraintAction {
        internal readonly UniqueDescription UniqueDescription;

        public UniqueRemoval(ConnectionInfo connectionInfo, UniqueDescription uniqueDescription) : base(connectionInfo) {
            this.UniqueDescription = uniqueDescription;
        }

        public override string Description {
            get {
                return string.Format("Removing unique key {0} in table {1}.",
                    UniqueDescription.FullName, UniqueDescription.TableName);
            }
        }

        internal override void Execute() {
            Constraints.RemoveUnique(UniqueDescription);
        }
    }
}