using System.Collections.Generic;

namespace Core.Descriptions {
    public class IndexDescription {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public IList<string> ColumnNames { get; set; }
        public string Name { get; set; }
        public bool Unique { get; set; }

        public string FullName {
            get {
                return string.IsNullOrWhiteSpace(Schema) ?
                    string.Format("{0}.{1}", TableName, Name) :
                    string.Format("{0}.{1}.{2}", Schema, TableName, Name);
            }
        }
    }
}