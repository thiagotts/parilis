using Core.Descriptions;

namespace Core.Actions {
    internal class ForeignKeyCreation : ConstraintAction {
        internal readonly ForeignKeyDescription ForeignKeyDescription;

        public ForeignKeyCreation(ConnectionInfo connectionInfo, ForeignKeyDescription foreignKeyDescription) : base(connectionInfo) {
            this.ForeignKeyDescription = foreignKeyDescription;
        }

        public override string Description {
            get {
                return string.Format("Creating foreign key {0} in table {1}.",
                    ForeignKeyDescription.FullName, ForeignKeyDescription.TableName);
            }
        }

        internal override void Execute() {
            Constraints.CreateForeignKey(ForeignKeyDescription);
        }
    }
}