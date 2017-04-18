using Core.Descriptions;

namespace Core.Actions {
    internal class ColumnModification : ColumnAction {
        private readonly ConnectionInfo connectionInfo;

        public ColumnModification(ConnectionInfo connectionInfo, ColumnDescription columnDescription) : base(connectionInfo, columnDescription) {
            this.connectionInfo = connectionInfo;
        }

        public override string Description =>
            $"Modifying column {ColumnDescription.FullName} to type {ColumnDescription.Type}" +
            $"{(string.IsNullOrWhiteSpace(ColumnDescription.Length) ? string.Empty : $" ({ColumnDescription.Length})")}" +
            $" ({(ColumnDescription.AllowsNull ? "Allows null values" : "Does not allow null values")}).";

        internal override void Execute() {
            var foreignKeys = Database.GetForeignKeysReferencing(ColumnDescription);
            var actionQueue = Components.Instance.GetComponent<ActionQueue>();

            foreach (var foreignKey in foreignKeys) {
                Constraints.RemoveForeignKey(foreignKey);
                actionQueue.Push(new ForeignKeyCreation(connectionInfo, foreignKey));
            }

            Columns.ChangeType(ColumnDescription);
        }
    }
}