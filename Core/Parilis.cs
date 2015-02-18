using System;
using System.Collections.Generic;
using System.Linq;
using Core.Descriptions;
using Core.Exceptions;
using Action = Core.Actions.Action;

namespace Core {
    public class Parilis {
        private DatabaseDescription actualDatabase;
        private DatabaseDescription referenceDatabase;
        private ActionIdentifier actionIdentifier;
        private Logger logger;

        public Parilis(DatabaseDescription actualDatabase, DatabaseDescription referenceDatabase) {
            this.actualDatabase = actualDatabase;
            this.referenceDatabase = referenceDatabase;
            actionIdentifier = new ActionIdentifier(actualDatabase, referenceDatabase);
            logger = new Logger();
        }

        public bool Run() {
            logger.Info("Parilis has started. Getting list of actions...");
            IList<Action> actions = actionIdentifier.GetActions();
            logger.Info(string.Format("{0} actions were identified."));

            if (!actions.Any()) {
                logger.Info("Parilis has finished with no pending actions.");
                return true;
            }

            bool actionsSuccessfullyExecuted = ExecuteActions(actions);
            if (!actionsSuccessfullyExecuted) return false;

            actions = GetRemainingActions();
            if (actions.Any()) {
                logger.Info(string.Format("Parilis has finished with {0} pending actions.", actions.Count));
                return false;
            }
            else {
                logger.Info("Parilis has finished with no pending actions.");
                return true;                
            }
        }

        private bool ExecuteActions(IEnumerable<Action> actions) {
            try {
                foreach (var action in actions) {
                    logger.Info(action.Description);
                    action.Execute();
                }
            }
            catch (ParilisException ex) {
                logger.Error("Error while executing action.", ex);
                return false;
            }
            catch (Exception ex) {
                logger.Error("Unexpected error while executing action.", ex);
                return false;
            }

            return true;
        }

        private IList<Action> GetRemainingActions() {
            actualDatabase = new DatabaseDescription(actualDatabase.ConnectionInfo);
            referenceDatabase = new DatabaseDescription(referenceDatabase.ConnectionInfo);
            return actionIdentifier.GetActions();
        }
    }
}