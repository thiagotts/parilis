using System;
using System.Collections.Generic;
using System.Linq;
using Core.Actions;
using Core.Descriptions;
using Action = Core.Actions.Action;

namespace Core {
    internal class ActionIdentifier {
        private readonly DatabaseDescription actualDatabase;
        private readonly DatabaseDescription referenceDatabase;
        private readonly ConnectionInfo connectionInfo;

        public ActionIdentifier(DatabaseDescription actualDatabase, DatabaseDescription referenceDatabase) {
            this.actualDatabase = actualDatabase;
            this.referenceDatabase = referenceDatabase;
            connectionInfo = actualDatabase.ConnectionInfo;
        }

        internal IList<Action> GetActions() {
            var actions = new List<Action>();

            foreach (var defaultRemoval in GetDefaultRemovals()) {
                actions.Add(defaultRemoval);
            }

            foreach (var uniqueRemoval in GetUniqueRemovals()) {
                actions.Add(uniqueRemoval);
            }

            foreach (var foreignKeyRemoval in GetForeignKeyRemovals()) {
                actions.Add(foreignKeyRemoval);
            }

            foreach (var primaryKeyRemoval in GetPrimaryKeyRemovals()) {
                actions.Add(primaryKeyRemoval);
            }

            foreach (var indexRemoval in GetIndexRemovals()) {
                actions.Add(indexRemoval);
            }

            foreach (var columnRemoval in GetColumnRemovals()) {
                actions.Add(columnRemoval);
            }

            foreach (var tableRemoval in GetTableRemovals()) {
                actions.Add(tableRemoval);
            }

            return actions;
        }

        private IEnumerable<Action> GetDefaultRemovals() {
            var defaultRemovals = new List<DefaultRemoval>();
            foreach (var defaultDescription in actualDatabase.Defaults) {
                if (!referenceDatabase.Defaults.Any(d => d.FullName.Equals(defaultDescription.FullName, StringComparison.InvariantCultureIgnoreCase)))
                    defaultRemovals.Add(new DefaultRemoval(connectionInfo, defaultDescription));
            }

            return defaultRemovals;
        }

        private IEnumerable<Action> GetUniqueRemovals() {
            var uniqueRemovals = new List<UniqueRemoval>();
            foreach (var uniqueKey in actualDatabase.UniqueKeys) {
                if (!referenceDatabase.UniqueKeys.Any(u => u.FullName.Equals(uniqueKey.FullName, StringComparison.InvariantCultureIgnoreCase)))
                    uniqueRemovals.Add(new UniqueRemoval(connectionInfo, uniqueKey));
            }

            return uniqueRemovals;
        }

        private IEnumerable<Action> GetForeignKeyRemovals() {
            var foreignKeyRemovals = new List<ForeignKeyRemoval>();
            foreach (var foreignKey in actualDatabase.ForeignKeys) {
                if (!referenceDatabase.ForeignKeys.Any(f => f.FullName.Equals(foreignKey.FullName, StringComparison.InvariantCultureIgnoreCase)))
                    foreignKeyRemovals.Add(new ForeignKeyRemoval(connectionInfo, foreignKey));
            }

            return foreignKeyRemovals;
        }

        private IEnumerable<Action> GetPrimaryKeyRemovals() {
            var primaryKeyRemovals = new List<PrimaryKeyRemoval>();
            foreach (var primaryKey in actualDatabase.PrimaryKeys) {
                if (!referenceDatabase.PrimaryKeys.Any(p => p.FullName.Equals(primaryKey.FullName, StringComparison.InvariantCultureIgnoreCase)))
                    primaryKeyRemovals.Add(new PrimaryKeyRemoval(connectionInfo, primaryKey));
            }

            return primaryKeyRemovals;
        }

        private IEnumerable<Action> GetIndexRemovals() {
            var indexRemovals = new List<IndexRemoval>();
            foreach (var index in actualDatabase.Indexes) {
                if (!referenceDatabase.Indexes.Any(i => i.FullName.Equals(index.FullName, StringComparison.InvariantCultureIgnoreCase)))
                    indexRemovals.Add(new IndexRemoval(connectionInfo, index));
            }

            return indexRemovals;
        }

        private IEnumerable<Action> GetColumnRemovals() {
            var columnRemovals = new List<ColumnRemoval>();

            foreach (var table in actualDatabase.Tables) {
                TableDescription referenceTable = referenceDatabase.Tables.SingleOrDefault(t => 
                    t.FullName.Equals(table.FullName, StringComparison.InvariantCultureIgnoreCase));

                if (referenceTable == null) continue;

                foreach (var column in table.Columns ?? new List<ColumnDescription>()) {
                    if (!referenceTable.Columns.Any(c => c.FullName.Equals(column.FullName, StringComparison.InvariantCultureIgnoreCase)))
                        columnRemovals.Add(new ColumnRemoval(connectionInfo, column));
                }                
            }

            return columnRemovals;
        }

        private IEnumerable<Action> GetTableRemovals() {
            var tableRemovals = new List<TableRemoval>();
            foreach (var table in actualDatabase.Tables) {
                if (!referenceDatabase.Tables.Any(t => t.FullName.Equals(table.FullName)))
                    tableRemovals.Add(new TableRemoval(connectionInfo, table));
            }

            return tableRemovals;
        }
    }
}