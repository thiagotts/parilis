using System.Collections.Generic;
using System.Linq;

namespace Core.Descriptions {
    public class UniqueDescription : ConstraintDescription {
        public IList<ColumnDescription> Columns { get; set; }

        public override string FullName {
            get { return string.IsNullOrWhiteSpace(Schema) ? Name : string.Format("{0}.{1}", Schema, Name); }
        }

        public override bool Equals(object other) {
            if (!(other is UniqueDescription)) return false;
            var uniqueDescription = other as UniqueDescription;

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