using Core.Descriptions;

namespace Core.Actions {
    internal class TableRemoval : TableAction {
        public TableRemoval(ConnectionInfo connectionInfo, TableDescription tableDescription) : base(connectionInfo, tableDescription) { }

        public override string Description {
            get { return string.Format("Removing table {0}.", TableDescription.FullName); }
        }

        internal override void Execute() {
            Tables.Remove(TableDescription.Schema, TableDescription.Name);
        }
    }
}