using Core.Descriptions;

namespace Core.Interfaces {
    interface IConstraint {
        void CreateUnique(UniqueDescription uniqueDescription);
        void RemoveUnique(UniqueDescription uniqueDescription);
        void CreateForeignKey(ForeignKeyDescription foreignKeyDescription);
        void RemoveForeignKey(ConstraintDescription foreignKeyDescription);
        void CreatePrimaryKey(PrimaryKeyDescription primaryKeyDescription);
        void RemovePrimaryKey(ConstraintDescription primaryKeyDescription);
        void CreateDefault(DefaultDescription defaultDescription);
        void RemoveDefault(ConstraintDescription defaultDescription);
    }
}
