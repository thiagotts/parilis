using System.Collections.Generic;

namespace Core.Descriptions {
    public class TableDescription : Description {
        public string Schema { get; set; }
        public string Name { get; set; }
        public IList<ColumnDescription> Columns { get; set; }

        public override string FullName {
            get { return string.IsNullOrWhiteSpace(Schema) ? Name : string.Format("{0}.{1}", Schema, Name); }
        }
    }
}