using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Castle.Core;
using Core;
using Core.Descriptions;
using Core.Exceptions;
using Core.Interfaces;

namespace SqlServer {
    [CastleComponent("SqlServer.Constraints", typeof (IConstraint), Lifestyle = LifestyleType.Transient)]
    public class Constraints : SqlServerEntity, IConstraint {
        public Constraints(ConnectionInfo database) {
            Initialize(database);
        }

        public void CreatePrimaryKey(PrimaryKeyDescription primaryKeyDescription) {
            var primaryKey = SqlServerDatabase.GetPrimaryKey(new TableDescription {
                Schema = primaryKeyDescription.Schema,
                Name = primaryKeyDescription.TableName
            });

            if (primaryKey != null) throw new MultiplePrimaryKeysException();

            primaryKey = SqlServerDatabase.GetPrimaryKey(primaryKeyDescription.Name, primaryKeyDescription.Schema);
            if (primaryKey != null) throw new InvalidConstraintNameException();

            var command = new SqlCommand(string.Format(@"
                ALTER TABLE [{0}].[{1}]
                ADD CONSTRAINT [{2}] PRIMARY KEY ({3})",
                primaryKeyDescription.Schema, primaryKeyDescription.TableName,
                primaryKeyDescription.Name, string.Join(",", primaryKeyDescription.Columns.Select(c => string.Format("[{0}]", c.Name)))));

            SqlServerDatabase.ExecuteNonQuery(command);
        }

        public void RemovePrimaryKey(PrimaryKeyDescription primaryKeyDescription) {
            var primaryKey = SqlServerDatabase.GetPrimaryKey(primaryKeyDescription.Name, primaryKeyDescription.Schema);
            if (primaryKey == null) throw new ConstraintNotFoundException();

            var foreignKeys = SqlServerDatabase.GetForeignKeysReferencing(primaryKeyDescription);
            if (foreignKeys.Any()) throw new ReferencedConstraintException();

            var command = new SqlCommand(string.Format(@"ALTER TABLE [{0}].[{1}] DROP CONSTRAINT [{2}]",
                primaryKeyDescription.Schema, primaryKeyDescription.TableName, primaryKeyDescription.Name));

            SqlServerDatabase.ExecuteNonQuery(command);
        }

        public void CreateForeignKey(ForeignKeyDescription foreignKeyDescription) {
            if (!DescriptionIsValid(foreignKeyDescription))
                throw new InvalidDescriptionException();

            if (ThereIsAnotherForeignKeyWithTheSameName(foreignKeyDescription))
                throw new InvalidConstraintNameException();

            if (!ReferencedColumnsAreValid(foreignKeyDescription))
                throw new InvalidReferenceColumnException();

            var command = new SqlCommand(string.Format(@"ALTER TABLE [{0}].[{1}] ADD CONSTRAINT [{2}] FOREIGN KEY ({3}) REFERENCES [{4}].[{5}]({6})",
                foreignKeyDescription.Schema, foreignKeyDescription.TableName, foreignKeyDescription.Name,
                string.Join(",", foreignKeyDescription.Columns.Select(c => string.Format("[{0}]", c.Key))),
                foreignKeyDescription.Columns.Values.First().Schema, foreignKeyDescription.Columns.Values.First().TableName,
                string.Join(",", foreignKeyDescription.Columns.Values.Select(v => string.Format("[{0}]", v.Name)))));

            SqlServerDatabase.ExecuteNonQuery(command);
        }

        public void RemoveForeignKey(ConstraintDescription foreignKeyDescription) {
            var foreignKeys = SqlServerDatabase.GetForeignKeys(new TableDescription {
                Schema = foreignKeyDescription.Schema,
                Name = foreignKeyDescription.TableName
            });

            if (!foreignKeys.Any(key => key.Schema.Equals(foreignKeyDescription.Schema) &&
                                        key.TableName.Equals(foreignKeyDescription.TableName) &&
                                        key.Name.Equals(foreignKeyDescription.Name)))
                throw new ConstraintNotFoundException();

            var command = new SqlCommand(string.Format(@"ALTER TABLE [{0}].[{1}] DROP CONSTRAINT [{2}]",
                foreignKeyDescription.Schema, foreignKeyDescription.TableName, foreignKeyDescription.Name));

            SqlServerDatabase.ExecuteNonQuery(command);
        }

        public void CreateUnique(UniqueDescription uniqueDescription) {
            var uniqueKey = SqlServerDatabase.GetUniqueKey(uniqueDescription.Name, uniqueDescription.Schema);
            if (uniqueKey != null) throw new InvalidConstraintNameException();

            if (!ReferencedColumnsAreValid(uniqueDescription))
                throw new InvalidReferenceColumnException();

            var command = new SqlCommand(string.Format(@"ALTER TABLE [{0}].[{1}] ADD CONSTRAINT [{2}] UNIQUE ({3})",
                uniqueDescription.Schema, uniqueDescription.TableName, uniqueDescription.Name,
                string.Join(",", uniqueDescription.Columns.Select(c => string.Format("[{0}]", c.Name)))));
            
            SqlServerDatabase.ExecuteNonQuery(command);
        }

        public void RemoveUnique(UniqueDescription uniqueDescription) {
            var uniqueKey = SqlServerDatabase.GetUniqueKey(uniqueDescription.Name, uniqueDescription.Schema);
            if (uniqueKey == null) throw new ConstraintNotFoundException();

            var foreignKeys = SqlServerDatabase.GetForeignKeysReferencing(uniqueDescription);
            if (foreignKeys.Any()) throw new ReferencedConstraintException();

            Database.ExecuteNonQuery(string.Format(@"ALTER TABLE {0}.{1} DROP CONSTRAINT {2}",
                uniqueDescription.Schema, uniqueDescription.TableName, uniqueDescription.Name));
        }

        public void CreateDefault(DefaultDescription defaultDescription) {
            var defaults = SqlServerDatabase.GetDefaults();

            if (defaults.Any(d => d.Name.Equals(defaultDescription.Name, StringComparison.InvariantCultureIgnoreCase) &&
                                  d.Schema.Equals(defaultDescription.Schema, StringComparison.InvariantCultureIgnoreCase)))
                throw new InvalidConstraintNameException();

            if (defaults.Any(d => d.Column.Equals(defaultDescription.Column) &&
                                  d.Schema.Equals(defaultDescription.Schema, StringComparison.InvariantCultureIgnoreCase) &&
                                  d.TableName.Equals(defaultDescription.TableName, StringComparison.InvariantCultureIgnoreCase)))
                throw new InvalidReferenceColumnException();

            Database.ExecuteNonQuery(string.Format(@"ALTER TABLE {0}.{1} ADD CONSTRAINT {2} DEFAULT {3} FOR {4}",
                defaultDescription.Schema, defaultDescription.TableName, defaultDescription.Name,
                defaultDescription.DefaultValue, defaultDescription.Column.Name));
        }

        public void RemoveDefault(DefaultDescription defaultDescription) {
            var defaultValue = SqlServerDatabase.GetDefault(defaultDescription.Name, defaultDescription.Schema);
            if (defaultValue == null) throw new ConstraintNotFoundException();

            Database.ExecuteNonQuery(string.Format(@"ALTER TABLE {0}.{1} DROP CONSTRAINT {2}",
                defaultDescription.Schema, defaultDescription.TableName, defaultDescription.Name));
        }

        private bool DescriptionIsValid(ForeignKeyDescription foreignKeyDescription) {
            foreach (var column in foreignKeyDescription.Columns.Values) {
                if (foreignKeyDescription.Columns.Values.Count(v =>
                    v.Name.Equals(column.Name, StringComparison.InvariantCultureIgnoreCase) &&
                    v.TableName.Equals(column.TableName, StringComparison.InvariantCultureIgnoreCase)) > 1) {
                    return false;
                }
            }

            return true;
        }

        private bool ThereIsAnotherForeignKeyWithTheSameName(ForeignKeyDescription foreignKeyDescription) {
            var foreignKeys = SqlServerDatabase.GetForeignKeys(new TableDescription {
                Schema = foreignKeyDescription.Schema,
                Name = foreignKeyDescription.TableName
            });

            return foreignKeys.Any(key => key.Name.Equals(foreignKeyDescription.Name, StringComparison.InvariantCultureIgnoreCase));
        }

        private bool ReferencedColumnsAreValid(ForeignKeyDescription foreignKeyDescription) {
            foreach (var column in foreignKeyDescription.Columns.Values) {
                var primaryKey = SqlServerDatabase.GetPrimaryKey(new TableDescription {
                    Schema = column.Schema,
                    Name = column.TableName
                });

                if (primaryKey == null || !primaryKey.Columns.Any(c => c.FullName.Equals(column.FullName, StringComparison.InvariantCultureIgnoreCase)))
                    return false;
            }

            return true;
        }

        private bool ReferencedColumnsAreValid(UniqueDescription uniqueDescription) {
            Database.Tables.Refresh();
            var table = Database.Tables[uniqueDescription.TableName, uniqueDescription.Schema];
            if (table == null) return false;

            var invalidTypes = new List<string> { "text", "ntext", "image", "xml", "geography", "geometry" };
            foreach (var columnDescription in uniqueDescription.Columns) {
                if (uniqueDescription.Columns.Count(c => c.FullName.Equals(columnDescription.FullName, StringComparison.InvariantCultureIgnoreCase)) > 1)
                    return false;

                var column = table.Columns[columnDescription.Name];
                if (column == null) return false;

                var description = SqlServerDatabase.GetColumn(uniqueDescription.Schema, uniqueDescription.TableName, columnDescription.Name);
                if (description == null) return false;

                if (description.Type.Equals("varchar", StringComparison.InvariantCultureIgnoreCase) &&
                    int.Parse(description.Length) > 900)
                    return false;

                if (description.Type.Equals("nvarchar", StringComparison.InvariantCultureIgnoreCase) &&
                    int.Parse(description.Length) > 400)
                    return false;

                if (invalidTypes.Any(t => t.Equals(description.Type, StringComparison.InvariantCultureIgnoreCase)))
                    return false;
            }

            if (SqlServerDatabase.TableHasDuplicatedValuesForColumns(uniqueDescription.Schema, uniqueDescription.TableName, 
                uniqueDescription.Columns.Select(c => c.Name).ToList()))
                return false;

            return true;
        }
    }
}