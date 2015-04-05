using System.Collections.Generic;
using Core.Descriptions;

namespace Core.Interfaces {
    public interface IDatabase {
        IList<string> GetSchemas();
        IList<TableDescription> GetTables();
        TableDescription GetTable(string schema, string tableName);
        ColumnDescription GetColumn(string schema, string tableName, string columnName);
        IList<IndexDescription> GetIndexes();
        IndexDescription GetIndex(string schema, string tableName, string indexName);
        IList<PrimaryKeyDescription> GetPrimaryKeys();
        IList<ForeignKeyDescription> GetForeignKeys();
        IList<ForeignKeyDescription> GetForeignKeysReferencing(ConstraintDescription constraintDescription);
        IList<UniqueDescription> GetUniqueKeys();
        IList<DefaultDescription> GetDefaults();
    }
}