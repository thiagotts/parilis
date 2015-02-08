namespace Core.Descriptions {
    public class ColumnDescription {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool AllowsNull { get; set; }
        public string MaximumSize { get; set; }
    }
}