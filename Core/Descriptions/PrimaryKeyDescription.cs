using System.Collections.Generic;
using System.Linq;

namespace Core.Descriptions {
    public class PrimaryKeyDescription : ConstraintDescription {
        public IList<string> ColumnNames { get; set; }

        public override string FullName {
            get { return string.IsNullOrWhiteSpace(Schema) ? Name : string.Format("{0}.{1}", Schema, Name); }
        }

        public override bool Equals(object other) {
            if (!(other is PrimaryKeyDescription)) return false;
            var indexDescription = other as PrimaryKeyDescription;
            return base.Equals(other) &&
                   ColumnNames != null &&
                   ColumnNames.Count > 0 &&
                   ColumnNames.Count == indexDescription.ColumnNames.Count &&
                   ColumnNames.All(c => indexDescription.ColumnNames.Any(f => f.Equals(c)));
        }

        public override int GetHashCode() {
            int hashCode = base.GetHashCode();
            foreach (var columnName in ColumnNames) {
                hashCode ^= columnName.GetHashCode();
            }

            return hashCode;
        }
    }
}