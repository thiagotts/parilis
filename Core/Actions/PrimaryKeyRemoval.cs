using Core.Descriptions;

namespace Core.Actions {
    internal class PrimaryKeyRemoval : ConstraintAction {
        internal readonly PrimaryKeyDescription PrimaryKeyDescription;

        public PrimaryKeyRemoval(ConnectionInfo connectionInfo, PrimaryKeyDescription primaryKeyDescription) : base(connectionInfo) {
            this.PrimaryKeyDescription = primaryKeyDescription;
        }

        public override string Description {
            get {
                return string.Format("Removing primary key {0} in table {1}.",
                    PrimaryKeyDescription.FullName, PrimaryKeyDescription.TableName);
            }
        }

        internal override void Execute() {
            Constraints.RemovePrimaryKey(PrimaryKeyDescription);
        }
    }
}