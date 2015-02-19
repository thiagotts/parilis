namespace Core.Actions {
    public abstract class Action {
        public abstract string Description { get; }
        internal abstract void Execute();

        public override string ToString() {
            return string.Format("{0}: {1}", GetType().Name, Description);
        }
    }
}