using log4net.Appender;
using log4net.Core;

namespace log4net.loggly
{
	public class LogglyBufferringAppender : BufferingAppenderSkeleton
	{
		public readonly string InputKeyProperty = "LogglyInputKey";

		public ILogglyFormatter Formatter = new LogglyFormatter();
		public ILogglyClient Client = new LogglyClient();

		private ILogglyAppenderConfig Config = new LogglyAppenderConfig();

		public string RootUrl { set { Config.RootUrl = value; } }
		public string InputKey { set { Config.InputKey = value; } }
		public string UserAgent { set { Config.UserAgent = value; } }
		public string LogMode { set { Config.LogMode = value; } }
		public int TimeoutInSeconds { set { Config.TimeoutInSeconds = value; } }
		public string Tag { set { Config.Tag = value; } }
		public int BufferSize { set { Config.BufferSize = value; } }

		protected override void Append(LoggingEvent loggingEvent)
		{
			Formatter.AppendAdditionalLoggingInformation(Config, loggingEvent);
			base.Append(loggingEvent);
		}

		protected override void SendBuffer(LoggingEvent[] loggingEvents)
		{
			Client.Send(Config, Formatter.ToJson(loggingEvents));
		}
	}
}