using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Core;
using Core.Descriptions;
using Core.Exceptions;
using Core.Interfaces;

namespace SqlServer {
    [CastleComponent("SqlServer.Indexes", typeof(IIndex), Lifestyle = LifestyleType.Transient)]
    public class Indexes : SqlServerEntity, IIndex {
        public Indexes(ConnectionInfo database) {
            Initialize(database);
        }

        public void Create(IndexDescription indexDescription) {
            if (!ReferencedColumnsAreInvalid(indexDescription))
                throw new InvalidReferenceColumnException();

            var index = SqlServerDatabase.GetIndex(indexDescription.Schema, indexDescription.TableName, indexDescription.Name);
            if (index != null) throw new InvalidIndexNameException();

            Database.ExecuteNonQuery(string.Format(@"CREATE {0} INDEX {1} ON {2}.{3} ({4})",
                indexDescription.Unique ? "UNIQUE" : string.Empty, indexDescription.Name, indexDescription.Schema,
                indexDescription.TableName, string.Join(",", indexDescription.ColumnNames)));
        }

        public void Remove(IndexDescription indexDescription) {
            var index = SqlServerDatabase.GetIndex(indexDescription.Schema, indexDescription.TableName, indexDescription.Name);
            if (index == null) throw new IndexNotFoundException();

            Database.ExecuteNonQuery(string.Format(@"DROP INDEX [{0}].[{1}].[{2}]",
                indexDescription.Schema, indexDescription.TableName, indexDescription.Name));
        }

        private bool ReferencedColumnsAreInvalid(IndexDescription indexDescription) {
            Database.Tables.Refresh();
            var table = Database.Tables[indexDescription.TableName, indexDescription.Schema];
            if (table == null) return false;

            var invalidTypes = new List<string> {"text", "ntext", "image", "xml", "geography", "geometry"};
            foreach (var columnName in indexDescription.ColumnNames) {
                if (indexDescription.ColumnNames.Count(name => name.Equals(columnName, StringComparison.InvariantCultureIgnoreCase)) > 1)
                    return false;

                var column = table.Columns[columnName];
                if (column == null) return false;

                var description = SqlServerDatabase.GetColumn(indexDescription.Schema, indexDescription.TableName, columnName);
                if (description == null) return false;

                if (description.Type.Equals("varchar", StringComparison.InvariantCultureIgnoreCase) &&
                    int.Parse(description.MaximumSize) > 900)
                    return false;

                if (description.Type.Equals("nvarchar", StringComparison.InvariantCultureIgnoreCase) &&
                    int.Parse(description.MaximumSize) > 400)
                    return false;

                if (invalidTypes.Any(t => t.Equals(description.Type, StringComparison.InvariantCultureIgnoreCase)))
                    return false;
            }

            return true;
        }
    }
}