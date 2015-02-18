namespace Core.Descriptions {
    public abstract class ConstraintDescription {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string Name { get; set; }

        public abstract string FullName { get; }
    }
}