using Core.Descriptions;

namespace Core.Interfaces {
    public interface IDatabase {
        PrimaryKeyDescription GetPrimaryKey(TableDescription table);
    }
}
