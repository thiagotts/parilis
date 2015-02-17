using Core.Interfaces;

namespace Core.Actions {
    public abstract class Action {
        protected IDatabase Database;

        public Action(ConnectionInfo connectionInfo) {
            Database = Components.Instance.GetComponent<IDatabase>(connectionInfo);
        }

        internal abstract void Execute();
    }
}