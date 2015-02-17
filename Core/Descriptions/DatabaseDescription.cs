using System.Collections.Generic;
using Core.Interfaces;

namespace Core.Descriptions {
    public class DatabaseDescription {
        public DatabaseDescription(ConnectionInfo connectionInfo) {
            var database = Components.Instance.GetComponent<IDatabase>(connectionInfo);
            //Schemas = database.GetSchemas();
        }

        internal IList<string> Schemas { get; private set; }
        internal IList<TableDescription> Tables { get; set; }
        internal IList<IndexDescription> Indexes { get; set; }
        internal IList<PrimaryKeyDescription> PrimaryKeys { get; set; }
        internal IList<ForeignKeyDescription> ForeignKeys { get; set; }
        internal IList<ForeignKeyDescription> UniqueKeys { get; set; }
        internal IList<ForeignKeyDescription> Defaults { get; set; }
    }
}