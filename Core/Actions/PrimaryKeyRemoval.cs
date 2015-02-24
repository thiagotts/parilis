using Core.Descriptions;

namespace Core.Actions {
    internal class PrimaryKeyRemoval : ConstraintAction {
        internal readonly PrimaryKeyDescription PrimaryKeyDescription;
        private readonly ConnectionInfo connectionInfo;

        public PrimaryKeyRemoval(ConnectionInfo connectionInfo, PrimaryKeyDescription primaryKeyDescription) : base(connectionInfo) {
            PrimaryKeyDescription = primaryKeyDescription;
            this.connectionInfo = connectionInfo;
        }

        public override string Description {
            get {
                return string.Format("Removing primary key {0} in table {1}.",
                    PrimaryKeyDescription.FullName, PrimaryKeyDescription.TableName);
            }
        }

        internal override void Execute() {
            var foreignKeys = Database.GetForeignKeysReferencing(PrimaryKeyDescription);
            var actionQueue = Components.Instance.GetComponent<ActionQueue>();

            foreach (var foreignKey in foreignKeys) {
                Constraints.RemoveForeignKey(foreignKey);
                actionQueue.Push(new ForeignKeyCreation(connectionInfo, foreignKey));
            }

            Constraints.RemovePrimaryKey(PrimaryKeyDescription);
        }
    }
}