using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;

namespace Core.Descriptions {
    public class DatabaseDescription {
        internal readonly ConnectionInfo ConnectionInfo;
        internal IList<string> Schemas { get; private set; }
        internal IList<TableDescription> Tables { get; private set; }
        internal IList<IndexDescription> Indexes { get; private set; }
        internal IList<PrimaryKeyDescription> PrimaryKeys { get; private set; }
        internal IList<ForeignKeyDescription> ForeignKeys { get; private set; }
        internal IList<UniqueDescription> UniqueKeys { get; private set; }
        internal IList<DefaultDescription> Defaults { get; private set; }

        public DatabaseDescription(ConnectionInfo connectionInfo) {
            var logger = new Logger();
            logger.Info(string.Format("Getting description of database {0} on {1}", connectionInfo.DatabaseName, connectionInfo.HostName));

            ConnectionInfo = connectionInfo;
            LoadDescription();
        }

        public DatabaseDescription(ConnectionInfo connectionInfo, string schema) {
            var logger = new Logger();
            logger.Info(string.Format("Getting description of schema {0} from database {1} on {2}", schema, connectionInfo.DatabaseName, connectionInfo.HostName));

            ConnectionInfo = connectionInfo;
            LoadDescription();
            FilterBySchema(schema);
        }

        private void FilterBySchema(string schema) {
            Schemas = Schemas.Where(s => s.Equals(schema, StringComparison.InvariantCultureIgnoreCase)).ToList();
            Tables = Tables.Where(t => !string.IsNullOrWhiteSpace(t.Schema) && t.Schema.Equals(schema, StringComparison.InvariantCultureIgnoreCase)).ToList();
            Indexes = Indexes.Where(i => !string.IsNullOrWhiteSpace(i.Schema) && i.Schema.Equals(schema, StringComparison.InvariantCultureIgnoreCase)).ToList();
            PrimaryKeys = PrimaryKeys.Where(p => !string.IsNullOrWhiteSpace(p.Schema) && p.Schema.Equals(schema, StringComparison.InvariantCultureIgnoreCase)).ToList();
            ForeignKeys = ForeignKeys.Where(f => !string.IsNullOrWhiteSpace(f.Schema) && f.Schema.Equals(schema, StringComparison.InvariantCultureIgnoreCase)).ToList();
            UniqueKeys = UniqueKeys.Where(u => !string.IsNullOrWhiteSpace(u.Schema) && u.Schema.Equals(schema, StringComparison.InvariantCultureIgnoreCase)).ToList();
            Defaults = Defaults.Where(d => !string.IsNullOrWhiteSpace(d.Schema) && d.Schema.Equals(schema, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        private void LoadDescription() {
            var database = Components.Instance.GetComponent<IDatabase>(ConnectionInfo);
            Schemas = database.GetSchemas();
            Tables = database.GetTables();
            Indexes = database.GetIndexes();
            PrimaryKeys = database.GetPrimaryKeys();
            ForeignKeys = database.GetForeignKeys();
            UniqueKeys = database.GetUniqueKeys();
            Defaults = database.GetDefaults();
        }
    }
}