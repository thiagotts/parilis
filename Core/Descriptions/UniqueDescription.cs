using System.Collections.Generic;
using System.Linq;

namespace Core.Descriptions {
    public class UniqueDescription : ConstraintDescription {
        private List<ColumnDescription> columns;

        public List<ColumnDescription> Columns => columns??(columns = new List<ColumnDescription>());

        public override string FullName => string.IsNullOrWhiteSpace(Schema) ? Name : $"{Schema}.{Name}";

        public override bool Equals(object other) {
            if (!(other is UniqueDescription)) return false;
            var uniqueDescription = (UniqueDescription) other;

            return base.Equals(other) &&
                   Columns != null &&
                   Columns.Count > 0 &&
                   Columns.Count == uniqueDescription.Columns.Count &&
                   Columns.All(c => uniqueDescription.Columns.Any(f => f.Equals(c)));
        }

        public override int GetHashCode() {
            var hashCode = base.GetHashCode();
            foreach (var columnName in Columns) {
                hashCode ^= columnName.GetHashCode();
            }

            return hashCode;
        }
    }
}