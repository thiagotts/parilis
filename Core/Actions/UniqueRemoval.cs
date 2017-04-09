using Core.Descriptions;

namespace Core.Actions {
    internal class UniqueRemoval : ConstraintAction {
        internal readonly UniqueDescription UniqueDescription;
        private readonly ConnectionInfo connectionInfo;

        public UniqueRemoval(ConnectionInfo connectionInfo, UniqueDescription uniqueDescription) : base(connectionInfo) {
            UniqueDescription = uniqueDescription;
            this.connectionInfo = connectionInfo;
        }

        public override string Description => $"Removing unique key {UniqueDescription.FullName} in table {UniqueDescription.TableName}.";

        internal override void Execute() {
            var foreignKeys = Database.GetForeignKeysReferencing(UniqueDescription);
            var actionQueue = Components.Instance.GetComponent<ActionQueue>();

            foreach (var foreignKey in foreignKeys) {
                Constraints.RemoveForeignKey(foreignKey);
                actionQueue.Push(new ForeignKeyCreation(connectionInfo, foreignKey));
            }

            Constraints.RemoveUnique(UniqueDescription);
        }
    }
}