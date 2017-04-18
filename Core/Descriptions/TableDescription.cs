using System.Collections.Generic;
using System.Linq;

namespace Core.Descriptions {
    public class TableDescription : Description {
        private List<ColumnDescription> columns;
        public string Schema { get; set; }
        public string Name { get; set; }

        public List<ColumnDescription> Columns => columns ?? (columns = new List<ColumnDescription>());

        public override string FullName => string.IsNullOrWhiteSpace(Schema) ? Name : $"{Schema}.{Name}";

        public override bool Equals(object other) {
            if (!(other is TableDescription)) return false;
            var tableDescription = (TableDescription) other;
            return base.Equals(other) &&
                   Columns.Count == tableDescription.Columns.Count &&
                   Columns.All(c => tableDescription.Columns.Any(t => t.Equals(c)));
        }

        public override int GetHashCode() {
            int hashCode = base.GetHashCode();
            foreach (var column in Columns) {
                hashCode ^= column.GetHashCode();
            }

            return hashCode;
        }
    }
}