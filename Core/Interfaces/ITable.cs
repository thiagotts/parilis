using Core.Descriptions;

namespace Core.Interfaces {
    public interface ITable {
        void Create(TableDescription tableDescription);
        void Remove(string schema, string tableName);
    }
}
