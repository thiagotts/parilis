using System;
using Core.Descriptions;
using Core.Exceptions;
using Action = Core.Actions.Action;

namespace Core {
    public class Parilis {
        private readonly ActionIdentifier actionIdentifier;
        private readonly Logger logger;

        public Parilis(DatabaseDescription actualDatabase, DatabaseDescription referenceDatabase) {
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

            logger.Info("Parilis has finished successfully with no pending actions.");
            return true;
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

        public bool AreAlreadyEqual(){
            return actionIdentifier.GetActions().Count == 0;
        }
    }
}