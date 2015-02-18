using Core.Descriptions;

namespace Core.Actions {
    internal class ForeignKeyRemoval : ConstraintAction {
        internal readonly ForeignKeyDescription ForeignKeyDescription;

        public ForeignKeyRemoval(ConnectionInfo connectionInfo, ForeignKeyDescription foreignKeyDescription) : base(connectionInfo) {
            this.ForeignKeyDescription = foreignKeyDescription;
        }

        public override string Description {
            get {
                return string.Format("Removing foreign key {0} in table {1}.",
                    ForeignKeyDescription.FullName, ForeignKeyDescription.TableName);
            }
        }

        internal override void Execute() {
            Constraints.RemoveForeignKey(ForeignKeyDescription);
        }
    }
}