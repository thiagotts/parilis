namespace Core.Actions {
    public abstract class Action {
        public abstract string Description { get; }
        internal abstract void Execute();

        public override string ToString() {
            return $"{GetType().Name}: {Description}";
        }
    }
}