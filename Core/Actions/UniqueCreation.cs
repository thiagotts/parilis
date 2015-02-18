using Core.Descriptions;

namespace Core.Actions {
    internal class UniqueCreation : ConstraintAction {
        internal readonly UniqueDescription UniqueDescription;

        public UniqueCreation(ConnectionInfo connectionInfo, UniqueDescription uniqueDescription) : base(connectionInfo) {
            this.UniqueDescription = uniqueDescription;
        }

        public override string Description {
            get {
                return string.Format("Creating unique key {0} in table {1}.",
                    UniqueDescription.FullName, UniqueDescription.TableName);
            }
        }

        internal override void Execute() {
            Constraints.CreateUnique(UniqueDescription);
        }
    }
}