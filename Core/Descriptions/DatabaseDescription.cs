using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Core.Interfaces;

namespace Core.Descriptions {
    public class DatabaseDescription {
        internal readonly ConnectionInfo ConnectionInfo;
        public IList<string> Schemas { get; internal set; }
        internal IList<TableDescription> Tables { get; set; }
        internal IList<IndexDescription> Indexes { get; set; }
        internal IList<PrimaryKeyDescription> PrimaryKeys { get; set; }
        internal IList<ForeignKeyDescription> ForeignKeys { get; set; }
        internal IList<UniqueDescription> UniqueKeys { get; set; }
        internal IList<DefaultDescription> Defaults { get; set; }

        private DatabaseDescription() {}

        public DatabaseDescription(ConnectionInfo connectionInfo) {
            var logger = new Logger();
            logger.Info(string.Format("Getting description of database {0} on {1}", connectionInfo.DatabaseName, connectionInfo.HostName));

            ConnectionInfo = connectionInfo;
            LoadDescription();
        }

        public DatabaseDescription FilterBySchema(params string[] schemas) {
            if (schemas.IsNullOrEmpty()) return this;

            var description = CreateEmptyDescription();

            foreach (var schema in schemas) {
                description.Schemas = description.Schemas.Concat(Schemas.Where(s => s.Equals(schema, StringComparison.InvariantCultureIgnoreCase))).ToList();
                description.Tables = description.Tables.Concat(Tables.Where(t => t.Schema.Equals(schema, StringComparison.InvariantCultureIgnoreCase))).ToList();
                description.Indexes = description.Indexes.Concat(Indexes.Where(i => i.Schema.Equals(schema, StringComparison.InvariantCultureIgnoreCase))).ToList();
                description.PrimaryKeys = description.PrimaryKeys.Concat(PrimaryKeys.Where(pk => pk.Schema.Equals(schema, StringComparison.InvariantCultureIgnoreCase))).ToList();
                description.ForeignKeys = description.ForeignKeys.Concat(ForeignKeys.Where(fk => fk.Schema.Equals(schema, StringComparison.InvariantCultureIgnoreCase))).ToList();
                description.UniqueKeys = description.UniqueKeys.Concat(UniqueKeys.Where(uk => uk.Schema.Equals(schema, StringComparison.InvariantCultureIgnoreCase))).ToList();
                description.Defaults = description.Defaults.Concat(Defaults.Where(d => d.Schema.Equals(schema, StringComparison.InvariantCultureIgnoreCase))).ToList();
            }

            return description;
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

        private DatabaseDescription CreateEmptyDescription() {
            return new DatabaseDescription(ConnectionInfo) {
                Schemas = new List<string>(),
                Tables = new List<TableDescription>(),
                Indexes = new List<IndexDescription>(),
                PrimaryKeys = new List<PrimaryKeyDescription>(),
                ForeignKeys = new List<ForeignKeyDescription>(),
                UniqueKeys = new List<UniqueDescription>(),
                Defaults = new List<DefaultDescription>()
            };
        }

        public bool IsEmpty {
            get {
                return Schemas.IsNullOrEmpty()
                       && Tables.IsNullOrEmpty()
                       && Indexes.IsNullOrEmpty()
                       && PrimaryKeys.IsNullOrEmpty()
                       && ForeignKeys.IsNullOrEmpty()
                       && UniqueKeys.IsNullOrEmpty()
                       && Defaults.IsNullOrEmpty();
            }
        }
    }
}