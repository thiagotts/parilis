using System.Collections.Generic;
using System.Linq;

namespace Core.Descriptions {
    public class IndexDescription : Description {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string Name { get; set; }
        public IList<string> ColumnNames { get; set; }
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
                   ColumnNames.Count == indexDescription.ColumnNames.Count &&
                   ColumnNames.All(c => indexDescription.ColumnNames.Any(f => f.Equals(c))) &&
                   Unique.Equals(indexDescription.Unique);
        }

        public override int GetHashCode() {
            int hashCode = base.GetHashCode() ^ Unique.GetHashCode();
            foreach (var columnName in ColumnNames) {
                hashCode ^= columnName.GetHashCode();
            }

            return hashCode;
        }
    }
}