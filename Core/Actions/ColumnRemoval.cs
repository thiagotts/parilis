using Core.Descriptions;

namespace Core.Actions {
    internal class ColumnRemoval : ColumnAction {
        public ColumnRemoval(ConnectionInfo connectionInfo, ColumnDescription columnDescription) : base(connectionInfo, columnDescription) { }

        public override string Description =>
            $"Removing column {ColumnDescription.FullName} of type {ColumnDescription.Type}" +
            $"{(string.IsNullOrWhiteSpace(ColumnDescription.Length) ? string.Empty : $" ({ColumnDescription.Length})")} " +
            $"({(ColumnDescription.AllowsNull ? " Allows null values" : " Does not allow null values")}).";

        internal override void Execute() {
            Columns.Remove(ColumnDescription);
        }
    }
}