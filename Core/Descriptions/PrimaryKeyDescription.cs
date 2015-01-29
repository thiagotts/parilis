using System.Collections.Generic;

namespace Core.Descriptions {
    public class PrimaryKeyDescription : ConstraintDescription {
        public IList<string> ColumnNames { get; set; }
    }
}