using System.Collections.Generic;

namespace Core.Descriptions {
    public class UniqueDescription : ConstraintDescription {
        public IList<string> ColumnNames { get; set; }

        public override string FullName {
            get { return string.IsNullOrWhiteSpace(Schema) ? Name : string.Format("{0}.{1}", Schema, Name); }
        }
    }
}