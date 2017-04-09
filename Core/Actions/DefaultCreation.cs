using Core.Descriptions;

namespace Core.Actions {
    internal class DefaultCreation : ConstraintAction {
        internal readonly DefaultDescription DefaultDescription;

        public DefaultCreation(ConnectionInfo connectionInfo, DefaultDescription defaultDescription) : base(connectionInfo) {
            DefaultDescription = defaultDescription;
        }

        public override string Description =>
            $"Creating default value constraint {DefaultDescription.FullName} in table {DefaultDescription.TableName}.";

        internal override void Execute() {
            Constraints.CreateDefault(DefaultDescription);
        }
    }
}