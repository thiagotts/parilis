namespace Core.Descriptions {
    public class ColumnDescription : Description {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool AllowsNull { get; set; }
        public string MaximumSize { get; set; }

        public override string FullName {
            get {
                return string.IsNullOrWhiteSpace(Schema) ?
                    string.Format("{0}.{1}", TableName, Name) :
                    string.Format("{0}.{1}.{2}", Schema, TableName, Name);
            }
        }
    }
}