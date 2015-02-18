namespace Core.Descriptions {
    public class DefaultDescription : ConstraintDescription {
        public string ColumnName { get; set; }
        public string DefaultValue { get; set; }

        public override string FullName {
            get { return string.IsNullOrWhiteSpace(Schema) ? Name : string.Format("{0}.{1}", Schema, Name); }
        }
    }
}