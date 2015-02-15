using System.Collections.Generic;

namespace Core.Descriptions {
    public class TableDescription {
        public string Schema { get; set; }
        public string Name { get; set; }
        public IList<ColumnDescription> Columns { get; set; }
    }
}