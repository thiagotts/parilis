using Core.Descriptions;

namespace Core.Actions {
    internal class ForeignKeyRemoval : ConstraintAction {
        private readonly ForeignKeyDescription foreignKeyDescription;

        public ForeignKeyRemoval(ConnectionInfo connectionInfo, ForeignKeyDescription foreignKeyDescription) : base(connectionInfo) {
            this.foreignKeyDescription = foreignKeyDescription;
        }

        internal override void Execute() {
            Constraints.RemoveForeignKey(foreignKeyDescription);
        }
    }
}