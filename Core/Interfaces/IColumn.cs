using Core.Descriptions;

namespace Core.Interfaces {
    public interface IColumn {
        void Create(ColumnDescription column);
        void Remove(ColumnDescription column);
        void ChangeType(ColumnDescription column);
    }
}