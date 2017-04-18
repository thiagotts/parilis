using System.Collections.Generic;
using System.Linq;

namespace Core.Descriptions {
    public class IndexDescription : Description {
        private List<ColumnDescription> columns;

        public string Schema { get; set; }
        public string TableName { get; set; }
        public string Name { get; set; }

        public List<ColumnDescription> Columns => columns??(columns = new List<ColumnDescription>());

        public bool Unique { get; set; }

        public override string FullName => string.IsNullOrWhiteSpace(Schema) ? $"{TableName}.{Name}" : $"{Schema}.{TableName}.{Name}";

        public override bool Equals(object other) {
            if (!(other is IndexDescription)) return false;
            var indexDescription = (IndexDescription) other;
            return base.Equals(other) &&
                   Columns != null &&
                   Columns.Count > 0 &&
                   Columns.Count == indexDescription.Columns.Count &&
                   Columns.All(c => indexDescription.Columns.Any(f => f.Equals(c))) &&
                   Unique.Equals(indexDescription.Unique);
        }

        public override int GetHashCode() {
            var hashCode = base.GetHashCode() ^ Unique.GetHashCode();
            foreach (var columnName in Columns) {
                hashCode ^= columnName.GetHashCode();
            }

            return hashCode;
        }
    }
}