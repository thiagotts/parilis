using System;
using Core.Descriptions;
using Core.Exceptions;
using Action = Core.Actions.Action;

namespace Core {
    public class Parilis {
        private DatabaseDescription actualDatabase;
        private DatabaseDescription referenceDatabase;
        private readonly ActionIdentifier actionIdentifier;
        private readonly Logger logger;

        public Parilis(DatabaseDescription actualDatabase, DatabaseDescription referenceDatabase) {
            this.actualDatabase = actualDatabase;
            this.referenceDatabase = referenceDatabase;
            actionIdentifier = new ActionIdentifier(actualDatabase, referenceDatabase);
            logger = new Logger();
        }

        public bool Run() {
            logger.Info("Parilis has started. Getting list of actions...");
            var actionQueue = actionIdentifier.GetActions();
            logger.Info(string.Format("{0} actions were initially identified.", actionQueue.Count));

            if (actionQueue.Count <= 0) {
                logger.Info("Parilis has finished with no pending actions.");
                return true;
            }

            var actionsSuccessfullyExecuted = ExecuteActions(actionQueue);
            if (!actionsSuccessfullyExecuted) {
                logger.Info("Parilis has finished with errors.");
                return false;
            }

            if (actionQueue.Count > 0) {
                logger.Info(string.Format("Parilis has finished with {0} pending actions.", actionQueue.Count));
                return false;
            }
            else {
                logger.Info("Parilis has finished with no pending actions.");
                return true;
            }
        }

        private bool ExecuteActions(ActionQueue actionQueue) {
            try {
                var actionCount = 0;
                Action action;
                while ((action = actionQueue.Pop()) != null) {
                    logger.Info(string.Format("Action {0} of {1}: {2}", ++actionCount, actionQueue.TotalCount, action.Description));
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

        private ActionQueue GetRemainingActions() {
            actualDatabase = new DatabaseDescription(actualDatabase.ConnectionInfo);
            referenceDatabase = new DatabaseDescription(referenceDatabase.ConnectionInfo);
            return actionIdentifier.GetActions();
        }
    }
}