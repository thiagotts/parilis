using Core.Descriptions;

namespace Core.Actions {
    internal class ForeignKeyRemoval : ConstraintAction {
        internal readonly ForeignKeyDescription ForeignKeyDescription;

        public ForeignKeyRemoval(ConnectionInfo connectionInfo, ForeignKeyDescription foreignKeyDescription) : base(connectionInfo) {
            ForeignKeyDescription = foreignKeyDescription;
        }

        public override string Description =>
            $"Removing foreign key {ForeignKeyDescription.FullName} in table {ForeignKeyDescription.TableName}.";

        internal override void Execute() {
            Constraints.RemoveForeignKey(ForeignKeyDescription);
        }
    }
}