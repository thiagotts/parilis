using Core.Descriptions;

namespace Core.Actions {
    internal class DefaultCreation : ConstraintAction {
        internal readonly DefaultDescription DefaultDescription;

        public DefaultCreation(ConnectionInfo connectionInfo, DefaultDescription defaultDescription) : base(connectionInfo) {
            this.DefaultDescription = defaultDescription;
        }

        public override string Description {
            get {
                return string.Format("Creating default value constraint {0} in table {1}.",
                    DefaultDescription.FullName, DefaultDescription.TableName);
            }
        }

        internal override void Execute() {
            Constraints.CreateDefault(DefaultDescription);
        }
    }
}