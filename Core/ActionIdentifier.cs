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

        internal ActionQueue GetActions() {
            var actionQueue = Components.Instance.GetComponent<ActionQueue>();
            actionQueue.Clear();
            GetRemovals(actionQueue);
            GetModifications(actionQueue);
            GetCreations(actionQueue);

            return actionQueue;
        }

        private void GetRemovals(ActionQueue actions) {
            foreach (var defaultRemoval in GetDefaultRemovals())
                actions.Push(defaultRemoval);

            foreach (var uniqueRemoval in GetUniqueRemovals())
                actions.Push(uniqueRemoval);

            foreach (var foreignKeyRemoval in GetForeignKeyRemovals())
                actions.Push(foreignKeyRemoval);

            foreach (var primaryKeyRemoval in GetPrimaryKeyRemovals())
                actions.Push(primaryKeyRemoval);

            foreach (var indexRemoval in GetIndexRemovals())
                actions.Push(indexRemoval);

            foreach (var columnRemoval in GetColumnRemovals())
                actions.Push(columnRemoval);

            foreach (var tableRemoval in GetTableRemovals())
                actions.Push(tableRemoval);

            foreach (var schemaRemoval in GetSchemaRemovals())
                actions.Push(schemaRemoval);
        }

        private IEnumerable<Action> GetDefaultRemovals() {
            var defaultRemovals = new List<DefaultRemoval>();
            foreach (var defaultDescription in actualDatabase.Defaults) {
                if (!referenceDatabase.Defaults.Any(d => d.Equals(defaultDescription)))
                    defaultRemovals.Add(new DefaultRemoval(connectionInfo, defaultDescription));
            }

            return defaultRemovals;
        }

        private IEnumerable<Action> GetUniqueRemovals() {
            var uniqueRemovals = new List<UniqueRemoval>();
            foreach (var uniqueKey in actualDatabase.UniqueKeys) {
                if (!referenceDatabase.UniqueKeys.Any(u => u.Equals(uniqueKey)))
                    uniqueRemovals.Add(new UniqueRemoval(connectionInfo, uniqueKey));
            }

            return uniqueRemovals;
        }

        private IEnumerable<Action> GetForeignKeyRemovals() {
            var foreignKeyRemovals = new List<ForeignKeyRemoval>();
            foreach (var foreignKey in actualDatabase.ForeignKeys) {
                if (!referenceDatabase.ForeignKeys.Any(f => f.Equals(foreignKey)))
                    foreignKeyRemovals.Add(new ForeignKeyRemoval(connectionInfo, foreignKey));
            }

            return foreignKeyRemovals;
        }

        private IEnumerable<Action> GetPrimaryKeyRemovals() {
            var primaryKeyRemovals = new List<PrimaryKeyRemoval>();
            foreach (var primaryKey in actualDatabase.PrimaryKeys) {
                if (!referenceDatabase.PrimaryKeys.Any(p => p.Equals(primaryKey)))
                    primaryKeyRemovals.Add(new PrimaryKeyRemoval(connectionInfo, primaryKey));
            }

            return primaryKeyRemovals;
        }

        private IEnumerable<Action> GetIndexRemovals() {
            var indexRemovals = new List<IndexRemoval>();
            foreach (var index in actualDatabase.Indexes) {
                if (!referenceDatabase.Indexes.Any(i => i.Equals(index)))
                    indexRemovals.Add(new IndexRemoval(connectionInfo, index));
            }

            return indexRemovals;
        }

        private IEnumerable<Action> GetColumnRemovals() {
            var columnRemovals = new List<ColumnRemoval>();

            foreach (var table in actualDatabase.Tables) {
                var referenceTable = referenceDatabase.Tables.SingleOrDefault(t =>
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
                if (!referenceDatabase.Tables.Any(t => t.FullName.Equals(table.FullName, StringComparison.InvariantCultureIgnoreCase)))
                    tableRemovals.Add(new TableRemoval(connectionInfo, table));
            }

            return tableRemovals;
        }

        private IEnumerable<Action> GetSchemaRemovals() {
            var schemaRemovals = new List<SchemaRemoval>();
            foreach (var schema in actualDatabase.Schemas) {
                if (!referenceDatabase.Schemas.Any(s => s.Equals(schema, StringComparison.InvariantCultureIgnoreCase)))
                    schemaRemovals.Add(new SchemaRemoval(connectionInfo, schema));
            }

            return schemaRemovals;
        }

        private void GetModifications(ActionQueue actions) {
            foreach (var table in actualDatabase.Tables) {
                var referenceTable = referenceDatabase.Tables.SingleOrDefault(t =>
                    t.FullName.Equals(table.FullName, StringComparison.InvariantCultureIgnoreCase));

                if (referenceTable == null) continue;

                foreach (var column in table.Columns ?? new List<ColumnDescription>()) {
                    if (!referenceTable.Columns.Any(c => c.FullName.Equals(column.FullName, StringComparison.InvariantCultureIgnoreCase)))
                        continue;

                    var referenceColumn = referenceTable.Columns.Single(c => c.FullName.Equals(column.FullName, StringComparison.InvariantCultureIgnoreCase));
                    if (!column.Equals(referenceColumn))
                        actions.Push(new ColumnModification(connectionInfo, referenceColumn));
                }
            }
        }

        private void GetCreations(ActionQueue actions) {
            foreach (var schemaCreation in GetSchemaCreations())
                actions.Push(schemaCreation);

            foreach (var tableCreation in GetTableCreations())
                actions.Push(tableCreation);

            foreach (var columnCreation in GetColumnCreations())
                actions.Push(columnCreation);

            foreach (var indexCreation in GetIndexCreations())
                actions.Push(indexCreation);

            foreach (var primaryKeyCreation in GetPrimaryKeyCreations())
                actions.Push(primaryKeyCreation);

            foreach (var foreignKeyCreation in GetForeignKeyCreations())
                actions.Push(foreignKeyCreation);

            foreach (var uniqueCreation in GetUniqueCreations())
                actions.Push(uniqueCreation);

            foreach (var defaultCreation in GetDefaultCreations())
                actions.Push(defaultCreation);
        }

        private IEnumerable<Action> GetSchemaCreations() {
            var schemaCreations = new List<SchemaCreation>();
            foreach (var schema in referenceDatabase.Schemas) {
                if (!actualDatabase.Schemas.Any(s => s.Equals(schema, StringComparison.InvariantCultureIgnoreCase)))
                    schemaCreations.Add(new SchemaCreation(connectionInfo, schema));
            }

            return schemaCreations;
        }

        private IEnumerable<Action> GetTableCreations() {
            var tableCreations = new List<TableCreation>();
            foreach (var table in referenceDatabase.Tables) {
                if (!actualDatabase.Tables.Any(t => t.FullName.Equals(table.FullName, StringComparison.InvariantCultureIgnoreCase)))
                    tableCreations.Add(new TableCreation(connectionInfo, table));
            }

            return tableCreations;
        }

        private IEnumerable<Action> GetColumnCreations() {
            var columnCreations = new List<ColumnCreation>();

            foreach (var table in referenceDatabase.Tables) {
                var actualTable = actualDatabase.Tables.SingleOrDefault(t =>
                    t.FullName.Equals(table.FullName, StringComparison.InvariantCultureIgnoreCase));

                if (actualTable == null) continue;

                foreach (var column in table.Columns ?? new List<ColumnDescription>()) {
                    if (!actualTable.Columns.Any(c => c.FullName.Equals(column.FullName, StringComparison.InvariantCultureIgnoreCase)))
                        columnCreations.Add(new ColumnCreation(connectionInfo, column));
                }
            }

            return columnCreations;
        }

        private IEnumerable<Action> GetIndexCreations() {
            var indexCreations = new List<IndexCreation>();
            foreach (var index in referenceDatabase.Indexes) {
                if (!actualDatabase.Indexes.Any(i => i.Equals(index)))
                    indexCreations.Add(new IndexCreation(connectionInfo, index));
            }

            return indexCreations;
        }

        private IEnumerable<Action> GetPrimaryKeyCreations() {
            var primaryKeyCreations = new List<PrimaryKeyCreation>();
            foreach (var primaryKey in referenceDatabase.PrimaryKeys) {
                if (!actualDatabase.PrimaryKeys.Any(p => p.Equals(primaryKey)))
                    primaryKeyCreations.Add(new PrimaryKeyCreation(connectionInfo, primaryKey));
            }

            return primaryKeyCreations;
        }

        private IEnumerable<Action> GetForeignKeyCreations() {
            var foreignKeyCreations = new List<ForeignKeyCreation>();
            foreach (var foreignKey in referenceDatabase.ForeignKeys) {
                if (!actualDatabase.ForeignKeys.Any(f => f.Equals(foreignKey)))
                    foreignKeyCreations.Add(new ForeignKeyCreation(connectionInfo, foreignKey));
            }

            return foreignKeyCreations;
        }

        private IEnumerable<Action> GetUniqueCreations() {
            var uniqueCreations = new List<UniqueCreation>();
            foreach (var uniqueKey in referenceDatabase.UniqueKeys) {
                if (!actualDatabase.UniqueKeys.Any(u => u.Equals(uniqueKey)))
                    uniqueCreations.Add(new UniqueCreation(connectionInfo, uniqueKey));
            }

            return uniqueCreations;
        }

        private IEnumerable<Action> GetDefaultCreations() {
            var defaultCreations = new List<DefaultCreation>();
            foreach (var defaultDescription in referenceDatabase.Defaults) {
                if (!actualDatabase.Defaults.Any(d => d.Equals(defaultDescription)))
                    defaultCreations.Add(new DefaultCreation(connectionInfo, defaultDescription));
            }

            return defaultCreations;
        }
    }
}