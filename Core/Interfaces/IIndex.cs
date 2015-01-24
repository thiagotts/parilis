using Core.Descriptions;

namespace Core.Interfaces {
    internal interface IIndex {
        void Create(IndexDescription indexDescription);
        void Remove(IndexDescription indexDescription);
    }
}