using Core.Descriptions;

namespace Core.Actions {
    internal class ForeignKeyCreation : ConstraintAction {
        internal readonly ForeignKeyDescription ForeignKeyDescription;

        public ForeignKeyCreation(ConnectionInfo connectionInfo, ForeignKeyDescription foreignKeyDescription) : base(connectionInfo) {
            ForeignKeyDescription = foreignKeyDescription;
        }

        public override string Description =>
            $"Creating foreign key {ForeignKeyDescription.FullName} in table {ForeignKeyDescription.TableName}.";

        internal override void Execute() {
            Constraints.CreateForeignKey(ForeignKeyDescription);
        }
    }
}