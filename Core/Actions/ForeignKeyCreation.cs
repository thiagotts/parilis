using Core.Descriptions;

namespace Core.Actions {
    internal class ForeignKeyCreation : ConstraintAction {
        private readonly ForeignKeyDescription foreignKeyDescription;

        public ForeignKeyCreation(ConnectionInfo connectionInfo, ForeignKeyDescription foreignKeyDescription) : base(connectionInfo) {
            this.foreignKeyDescription = foreignKeyDescription;
        }

        internal override void Execute() {
            Constraints.CreateForeignKey(foreignKeyDescription);
        }
    }
}