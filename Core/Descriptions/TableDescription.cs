using System.Collections.Generic;

namespace Core.Descriptions {
    public class TableDescription {
        public string Schema { get; set; }
        public string Name { get; set; }
        public IList<ColumnDescription> Columns { get; set; }

        public string FullName {
            get { return string.IsNullOrWhiteSpace(Schema) ? Name : string.Format("{0}.{1}", Schema, Name); }
        }
    }
}