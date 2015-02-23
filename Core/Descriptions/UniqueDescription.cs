using System.Collections.Generic;
using System.Linq;

namespace Core.Descriptions {
    public class UniqueDescription : ConstraintDescription {
        public IList<string> ColumnNames { get; set; }

        public override string FullName {
            get { return string.IsNullOrWhiteSpace(Schema) ? Name : string.Format("{0}.{1}", Schema, Name); }
        }

        public override bool Equals(object other) {
            if (!(other is UniqueDescription)) return false;
            var uniqueDescription = other as UniqueDescription;
            return base.Equals(other) &&
                   ColumnNames != null &&
                   ColumnNames.Count > 0 &&
                   ColumnNames.Count == uniqueDescription.ColumnNames.Count &&
                   ColumnNames.All(c => uniqueDescription.ColumnNames.Any(f => f.Equals(c)));
        }

        public override int GetHashCode() {
            var hashCode = base.GetHashCode();
            foreach (var columnName in ColumnNames) {
                hashCode ^= columnName.GetHashCode();
            }

            return hashCode;
        }
    }
}