using System.Collections.Generic;
using Core.Interfaces;

namespace Core.Descriptions {
    public class DatabaseDescription {
        internal ConnectionInfo ConnectionInfo;

        public DatabaseDescription(ConnectionInfo connectionInfo) {
            ConnectionInfo = connectionInfo;
            var database = Components.Instance.GetComponent<IDatabase>(connectionInfo);
            Schemas = database.GetSchemas();
            Tables = database.GetTables();
            Indexes = database.GetIndexes();
            PrimaryKeys = database.GetPrimaryKeys();
            ForeignKeys = database.GetForeignKeys();
            UniqueKeys = database.GetUniqueKeys();
            Defaults = database.GetDefaults();
        }

        internal IList<string> Schemas { get; private set; }
        internal IList<TableDescription> Tables { get; private set; }
        internal IList<IndexDescription> Indexes { get; private set; }
        internal IList<PrimaryKeyDescription> PrimaryKeys { get; private set; }
        internal IList<ForeignKeyDescription> ForeignKeys { get; private set; }
        internal IList<UniqueDescription> UniqueKeys { get; private set; }
        internal IList<DefaultDescription> Defaults { get; private set; }
    }
}