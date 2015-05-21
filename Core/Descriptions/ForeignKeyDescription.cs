using System.Collections.Generic;
using System.Linq;

namespace Core.Descriptions {
    public class ForeignKeyDescription : ConstraintDescription {
        public IDictionary<string, ColumnDescription> Columns;

        public override string FullName {
            get { return string.IsNullOrWhiteSpace(Schema) ? Name : string.Format("{0}.{1}", Schema, Name); }
        }

        public override bool Equals(object other) {
            if (!(other is ForeignKeyDescription)) return false;
            var foreignKeyDescription = other as ForeignKeyDescription;
            return base.Equals(other) &&
                   Columns != null &&
                   Columns.Count > 0 &&
                   Columns.Count == foreignKeyDescription.Columns.Count &&
                   Columns.Keys.All(c => foreignKeyDescription.Columns.Keys.Any(f => f.Equals(c))) &&
                   Columns.Values.All(c => foreignKeyDescription.Columns.Values.Any(f => f.Equals(c)));
        }
    }
}