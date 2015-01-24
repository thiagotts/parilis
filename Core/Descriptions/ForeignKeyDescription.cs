namespace Core.Descriptions {
    public class ForeignKeyDescription : ConstraintDescription {
        public string ColumnName { get; set; }
        public ColumnDescription ReferenceColumn { get; set; }
    }
}