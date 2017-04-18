using System.Collections.Generic;
using System.Linq;

namespace Core.Descriptions {
    public class PrimaryKeyDescription : ConstraintDescription {
        private List<ColumnDescription> columns;

        public List<ColumnDescription> Columns => columns ?? (columns = new List<ColumnDescription>());

        public override string FullName => string.IsNullOrWhiteSpace(Schema) ? Name : $"{Schema}.{Name}";

        public override bool Equals(object other) {
            if (!(other is PrimaryKeyDescription)) return false;
            var indexDescription = (PrimaryKeyDescription) other;
            return base.Equals(other) &&
                   Columns != null &&
                   Columns.Count > 0 &&
                   Columns.Count == indexDescription.Columns.Count &&
                   Columns.All(c => indexDescription.Columns.Any(f => f.Equals(c)));
        }

        public override int GetHashCode() {
            int hashCode = base.GetHashCode();
            foreach (var columnName in Columns) {
                hashCode ^= columnName.GetHashCode();
            }

            return hashCode;
        }
    }
}