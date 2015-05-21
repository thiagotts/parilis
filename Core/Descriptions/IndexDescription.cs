using System.Collections.Generic;
using System.Linq;

namespace Core.Descriptions {
    public class IndexDescription : Description {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string Name { get; set; }
        public IList<ColumnDescription> Columns { get; set; }
        public bool Unique { get; set; }

        public override string FullName {
            get {
                return string.IsNullOrWhiteSpace(Schema) ?
                    string.Format("{0}.{1}", TableName, Name) :
                    string.Format("{0}.{1}.{2}", Schema, TableName, Name);
            }
        }

        public override bool Equals(object other) {
            if (!(other is IndexDescription)) return false;
            var indexDescription = other as IndexDescription;
            return base.Equals(other) &&
                   Columns != null &&
                   Columns.Count > 0 &&
                   Columns.Count == indexDescription.Columns.Count &&
                   Columns.All(c => indexDescription.Columns.Any(f => f.Equals(c))) &&
                   Unique.Equals(indexDescription.Unique);
        }

        public override int GetHashCode() {
            int hashCode = base.GetHashCode() ^ Unique.GetHashCode();
            foreach (var columnName in Columns) {
                hashCode ^= columnName.GetHashCode();
            }

            return hashCode;
        }
    }
}