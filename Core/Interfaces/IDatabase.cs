using System.Collections.Generic;
using Core.Descriptions;

namespace Core.Interfaces {
    public interface IDatabase {
        IList<string> GetSchemas();
        IList<TableDescription> GetTables();
        TableDescription GetTable(string schema, string tableName);
        ColumnDescription GetColumn(string schema, string tableName, string columnName);
        IList<IndexDescription> GetIndexes();
        IList<IndexDescription> GetIndexes(string schema, string tableName);
        IndexDescription GetIndex(string schema, string tableName, string indexName);
        IList<PrimaryKeyDescription> GetPrimaryKeys();
        PrimaryKeyDescription GetPrimaryKey(TableDescription table);
        IList<ForeignKeyDescription> GetForeignKeys();
        IList<ForeignKeyDescription> GetForeignKeys(TableDescription tableDescription);
        IList<ForeignKeyDescription> GetForeignKeysReferencing(ConstraintDescription primaryKeyDescription);
        IList<UniqueDescription> GetUniqueKeys();
        IList<UniqueDescription> GetUniqueKeys(TableDescription tableDescription);
        UniqueDescription GetUniqueKey(string uniqueKeyName);
        IList<DefaultDescription> GetDefaults();
        DefaultDescription GetDefault(string defaultName, string schema);
    }
}