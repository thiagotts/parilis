using Core.Descriptions;

namespace Core.Actions {
    internal class ForeignKeyCreation : ConstraintAction {
        internal readonly ForeignKeyDescription ForeignKeyDescription;

        public ForeignKeyCreation(ConnectionInfo connectionInfo, ForeignKeyDescription foreignKeyDescription) : base(connectionInfo) {
            this.ForeignKeyDescription = foreignKeyDescription;
        }

        internal override void Execute() {
            Constraints.CreateForeignKey(ForeignKeyDescription);
        }
    }
}