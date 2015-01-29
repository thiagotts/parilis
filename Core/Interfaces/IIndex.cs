using Core.Descriptions;

namespace Core.Interfaces {
    public interface IIndex {
        void Create(IndexDescription indexDescription);
        void Remove(IndexDescription indexDescription);
    }
}