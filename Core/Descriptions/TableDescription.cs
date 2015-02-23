using System.Collections.Generic;
using System.Linq;

namespace Core.Descriptions {
    public class TableDescription : Description {
        public string Schema { get; set; }
        public string Name { get; set; }
        public IList<ColumnDescription> Columns { get; set; }

        public override string FullName {
            get { return string.IsNullOrWhiteSpace(Schema) ? Name : string.Format("{0}.{1}", Schema, Name); }
        }

        public override bool Equals(object other) {
            if (!(other is TableDescription)) return false;
            var tableDescription = other as TableDescription;
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