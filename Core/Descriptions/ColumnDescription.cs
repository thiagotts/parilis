namespace Core.Descriptions {
    public class ColumnDescription : Description {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Length { get; set; }
        public bool AllowsNull { get; set; }
        public bool IsIdentity { get; set; }

        public override string FullName {
            get {
                return string.IsNullOrWhiteSpace(Schema) ?
                    string.Format("{0}.{1}", TableName, Name) :
                    string.Format("{0}.{1}.{2}", Schema, TableName, Name);
            }
        }

        public override bool Equals(object other) {
            if (!(other is ColumnDescription)) return false;
            var columnDescription = other as ColumnDescription;
            return base.Equals(other) &&
                   !string.IsNullOrWhiteSpace(Type) &&
                   Type.Equals(columnDescription.Type) &&
                   ((string.IsNullOrWhiteSpace(Length) && string.IsNullOrWhiteSpace(columnDescription.Length)) ||
                    Length.Equals(columnDescription.Length)) &&
                   AllowsNull.Equals(columnDescription.AllowsNull) &&
                   IsIdentity.Equals(columnDescription.IsIdentity);
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^
                   Type.GetHashCode() ^
                   Length.GetHashCode() ^
                   AllowsNull.GetHashCode();
        }
    }
}