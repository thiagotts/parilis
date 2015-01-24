namespace Core.Interfaces {
    public interface ISchema {
        void Create(string schemaName);
        void Remove(string schemaName);
    }
}