using System.Collections.Generic;

namespace Core.Descriptions {
    public class UniqueDescription : ConstraintDescription {
        public IEnumerable<string> ColumnNames { get; set; }
        public bool Clustered { get; set; }
    }
}