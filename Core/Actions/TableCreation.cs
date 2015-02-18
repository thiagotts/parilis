using Core.Descriptions;

namespace Core.Actions {
    internal class TableCreation : TableAction {
        public TableCreation(ConnectionInfo connectionInfo, TableDescription tableDescription) : base(connectionInfo, tableDescription) { }

        public override string Description {
            get { return string.Format("Creating table {0}.", TableDescription.FullName); }
        }

        internal override void Execute() {
            Tables.Create(TableDescription);
        }
    }
}