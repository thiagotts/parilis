using Core.Descriptions;

namespace Core.Actions {
    internal class ColumnModification : ColumnAction {
        private readonly ConnectionInfo connectionInfo;

        public ColumnModification(ConnectionInfo connectionInfo, ColumnDescription columnDescription) : base(connectionInfo, columnDescription) {
            this.connectionInfo = connectionInfo;
        }

        public override string Description {
            get {
                return string.Format("Modifying column {0} to type {1}.",
                    ColumnDescription.FullName,
                    string.Format("{0}{1} ({2})",
                        ColumnDescription.Type,
                        string.IsNullOrWhiteSpace(ColumnDescription.Length) ? string.Empty : string.Format("({0})", ColumnDescription.Length),
                        ColumnDescription.AllowsNull ? "Allows null values" : "Does not allow null values"));
            }
        }

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