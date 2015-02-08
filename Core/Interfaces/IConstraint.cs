using Core.Descriptions;

namespace Core.Interfaces {
    public interface IConstraint {
        void CreatePrimaryKey(PrimaryKeyDescription primaryKeyDescription);
        void RemovePrimaryKey(PrimaryKeyDescription primaryKeyDescription);
        void CreateForeignKey(ForeignKeyDescription foreignKeyDescription);
        void RemoveForeignKey(ConstraintDescription foreignKeyDescription);
        void CreateUnique(UniqueDescription uniqueDescription);
        void RemoveUnique(UniqueDescription uniqueDescription);
        void CreateDefault(DefaultDescription defaultDescription);
        void RemoveDefault(DefaultDescription defaultDescription);
    }
}
