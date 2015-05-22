using System.Collections.Generic;
using System.Linq;

namespace Core.Descriptions {
    public class PrimaryKeyDescription : ConstraintDescription {
        public IList<ColumnDescription> Columns { get; set; }

        public override string FullName {
            get { return string.IsNullOrWhiteSpace(Schema) ? Name : string.Format("{0}.{1}", Schema, Name); }
        }

        public override bool Equals(object other) {
            if (!(other is PrimaryKeyDescription)) return false;
            var indexDescription = other as PrimaryKeyDescription;
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