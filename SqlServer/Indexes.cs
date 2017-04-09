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
    [CastleComponent("SqlServer.Indexes", typeof (IIndex), Lifestyle = LifestyleType.Transient)]
    public class Indexes : SqlServerEntity, IIndex {
        public Indexes(ConnectionInfo database) {
            Initialize(database);
        }

        public void Create(IndexDescription indexDescription) {
            if (!ReferencedColumnsAreInvalid(indexDescription))
                throw new InvalidReferenceColumnException();

            var index = SqlServerDatabase.GetIndex(indexDescription.Schema, indexDescription.TableName, indexDescription.Name);
            if (index != null) throw new InvalidIndexNameException();

            var createUniqueOrIndexCommandText = $@"CREATE {(indexDescription.Unique ? "UNIQUE" : string.Empty)} 
                                                    INDEX [{indexDescription.Name}] ON [{indexDescription.Schema}].[{indexDescription.TableName}] 
                                                    ({string.Join(",", indexDescription.Columns.Select(c => $"[{c.Name}]"))})";

            var command = new SqlCommand(createUniqueOrIndexCommandText);

            SqlServerDatabase.ExecuteNonQuery(command);
        }

        public void Remove(IndexDescription indexDescription) {
            var index = SqlServerDatabase.GetIndex(indexDescription.Schema, indexDescription.TableName, indexDescription.Name);
            if (index == null) throw new IndexNotFoundException();

            var dropIndexCommandText = $@"DROP INDEX [{indexDescription.Schema}].[{indexDescription.TableName}].[{indexDescription.Name}]";
            var command = new SqlCommand(dropIndexCommandText);

            SqlServerDatabase.ExecuteNonQuery(command);
        }

        private bool ReferencedColumnsAreInvalid(IndexDescription indexDescription) {
            Database.Tables.Refresh();
            var table = Database.Tables[indexDescription.TableName, indexDescription.Schema];
            if (table == null) return false;

            var invalidTypes = new List<string> {"text", "ntext", "image", "xml", "geography", "geometry"};
            foreach (var columnDescription in indexDescription.Columns) {
                if (indexDescription.Columns.Count(c => c.Name.Equals(columnDescription.Name, StringComparison.InvariantCultureIgnoreCase)) > 1)
                    return false;

                var column = table.Columns[columnDescription.Name];
                if (column == null) return false;

                var description = SqlServerDatabase.GetColumn(indexDescription.Schema, indexDescription.TableName, columnDescription.Name);
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

            return true;
        }
    }
}