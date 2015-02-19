using System;
using System.Text;
using log4net;

namespace Core {
    public class Logger {
        private readonly ILog log;

        public Logger() {
            log = LogManager.GetLogger(GetType());
        }

        public void Info(string message) {
            log.Info(message);
        }

        public void Error(string message, Exception exception) {
            var builder = new StringBuilder(message).AppendLine();
            builder = DescribeException(builder, exception);
            log.Error(builder.ToString());
        }

        private StringBuilder DescribeException(StringBuilder builder, Exception exception) {
            builder.AppendLine(@"### ")
                .AppendLine(exception.GetType().FullName)
                .AppendLine(exception.Message)
                .AppendLine(exception.StackTrace ?? @"Stack trace is unavailable.");

            if (exception.InnerException != null) {
                builder = DescribeException(builder, exception.InnerException);
            }

            return builder;
        }
    }
}