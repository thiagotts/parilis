using Core.Descriptions;

namespace Core.Actions {
    internal class ColumnCreation : ColumnAction {
        public ColumnCreation(ConnectionInfo connectionInfo, ColumnDescription columnDescription) : base(connectionInfo, columnDescription) { }

        public override string Description {
            get {
                return string.Format("Creating column {0} of type {1}.",
                    ColumnDescription.FullName,
                    string.Format("{0}{1} ({2})",
                        ColumnDescription.Type,
                        string.IsNullOrWhiteSpace(ColumnDescription.Length) ? string.Empty : string.Format("({0})", ColumnDescription.Length),
                        ColumnDescription.AllowsNull ? "Allows null values" : "Does not allow null values"));
            }
        }

        internal override void Execute() {
            Columns.Create(ColumnDescription);
        }
    }
}