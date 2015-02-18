using Core.Descriptions;

namespace Core.Actions {
    internal class PrimaryKeyCreation : ConstraintAction {
        internal readonly PrimaryKeyDescription PrimaryKeyDescription;

        public PrimaryKeyCreation(ConnectionInfo connectionInfo, PrimaryKeyDescription primaryKeyDescription) : base(connectionInfo) {
            this.PrimaryKeyDescription = primaryKeyDescription;
        }

        public override string Description {
            get {
                return string.Format("Creating primary key {0} in table {1}.",
                    PrimaryKeyDescription.FullName, PrimaryKeyDescription.TableName);
            }
        }

        internal override void Execute() {
            Constraints.CreatePrimaryKey(PrimaryKeyDescription);
        }
    }
}