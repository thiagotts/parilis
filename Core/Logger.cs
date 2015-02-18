using System;
using System.Diagnostics;
using System.Text;
using log4net;

namespace Core {
    public class Logger {
        private readonly ILog log;

        public Logger() {
            log = LogManager.GetLogger(GetType());
        }

        public void Info(string message) {
            var builder = new StringBuilder(message).AppendLine();
            builder = DescribeSource(builder);
            log.Info(builder.ToString());
        }

        public void Error(string message, Exception exception) {
            var builder = new StringBuilder(message).AppendLine();
            builder = DescribeSource(builder);
            builder = DescribeException(builder, exception);

            log.Error(builder.ToString());
        }

        private StringBuilder DescribeSource(StringBuilder builder) {
            var stackFrames = new StackTrace(true).GetFrames();
            if (stackFrames != null && stackFrames.Length > 2) {
                builder.Append(@"Source: ").Append(stackFrames[2]);
            }

            return builder;
        }

        private StringBuilder DescribeException(StringBuilder builder, Exception exception) {
            builder.AppendLine(@"### ")
                .AppendLine(exception.GetType().FullName)
                .AppendLine(exception.Message)
                .AppendLine(exception.StackTrace ?? @"Stack trace is unvailable.");

            if (exception.InnerException != null) {
                builder = DescribeException(builder, exception.InnerException);
            }

            return builder;
        }
    }
}