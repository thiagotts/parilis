using System.Collections.Generic;

namespace Core.Descriptions {
    public class PrimaryKeyDescription : ConstraintDescription {
        public IEnumerable<string> ColumnNames { get; set; }
    }
}