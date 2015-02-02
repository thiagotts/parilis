using System.Collections.Generic;

namespace Core.Descriptions {
    public class ForeignKeyDescription : ConstraintDescription {
        public IDictionary<string, ColumnDescription> Columns;
    }
}