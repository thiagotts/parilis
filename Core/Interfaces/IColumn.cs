using Core.Descriptions;

namespace Core.Interfaces {
    interface IColumn {
        void Create(ColumnDescription column);
        void Remove(ColumnDescription column);
        void ChangeType(ColumnDescription from, ColumnDescription to);
    }
}
