using System.Collections.Generic;

namespace Core.Descriptions {
    public class ForeignKeyDescription : ConstraintDescription {
        public IDictionary<string, ColumnDescription> Columns;

        public override string FullName {
            get { return string.IsNullOrWhiteSpace(Schema) ? Name : string.Format("{0}.{1}", Schema, Name); }
        }
    }
}