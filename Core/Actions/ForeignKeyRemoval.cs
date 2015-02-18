using Core.Descriptions;

namespace Core.Actions {
    internal class ForeignKeyRemoval : ConstraintAction {
        internal readonly ForeignKeyDescription ForeignKeyDescription;

        public ForeignKeyRemoval(ConnectionInfo connectionInfo, ForeignKeyDescription foreignKeyDescription) : base(connectionInfo) {
            this.ForeignKeyDescription = foreignKeyDescription;
        }

        internal override void Execute() {
            Constraints.RemoveForeignKey(ForeignKeyDescription);
        }
    }
}