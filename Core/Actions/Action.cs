namespace Core.Actions {
    public abstract class Action {
        public abstract string Description { get; }
        internal abstract void Execute();
    }
}