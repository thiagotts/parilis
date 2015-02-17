using System.Collections.Generic;

namespace Core.Descriptions {
    public class UniqueDescription : ConstraintDescription {
        public IList<string> ColumnNames { get; set; }
    }
}