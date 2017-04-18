using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Core.Descriptions {
    public class ForeignKeyDescription : ConstraintDescription {
        private IDictionary<string, ColumnDescription> columns;

        public IDictionary<string, ColumnDescription> Columns {
            get { return columns ?? (columns = new ConcurrentDictionary<string, ColumnDescription>()); }
            set { columns = value; }
        }

        public override string FullName => string.IsNullOrWhiteSpace(Schema) ? Name : $"{Schema}.{Name}";

        public override bool Equals(object other) {
            if (!(other is ForeignKeyDescription)) return false;
            var foreignKeyDescription = (ForeignKeyDescription) other;
            return base.Equals(other) &&
                   Columns != null &&
                   Columns.Count > 0 &&
                   Columns.Count == foreignKeyDescription.Columns.Count &&
                   Columns.Keys.All(c => foreignKeyDescription.Columns.Keys.Any(f => f.Equals(c))) &&
                   Columns.Values.All(c => foreignKeyDescription.Columns.Values.Any(f => f.Equals(c)));
        }

        protected bool Equals(ForeignKeyDescription other) {
            return base.Equals(other) && Equals(Columns, other.Columns);
        }

        public override int GetHashCode() {
            unchecked {
                return (base.GetHashCode() * 397) ^ (Columns != null ? Columns.GetHashCode() : 0);
            }
        }
    }
}