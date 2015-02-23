namespace Core.Descriptions {
    public abstract class ConstraintDescription : Description {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string Name { get; set; }
    }
}