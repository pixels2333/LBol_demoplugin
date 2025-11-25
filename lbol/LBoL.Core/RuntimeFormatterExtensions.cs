using System;
namespace LBoL.Core
{
	public static class RuntimeFormatterExtensions
	{
		public static string RuntimeFormat(this string format, RuntimeFormatterArgmentHandler argumentHandler)
		{
			return new RuntimeFormatter(format).Format(argumentHandler);
		}
		public static string RuntimeFormat(this string format, Func<string, string> valueGetter)
		{
			return new RuntimeFormatter(format).Format((string key, string _) => valueGetter.Invoke(key));
		}
		internal static string RuntimeFormat(this string format, GameEntityFormatWrapper formatWrapper)
		{
			return new RuntimeFormatter(format).Format(formatWrapper);
		}
		public static string RuntimeFormat(this string format, object obj)
		{
			return new RuntimeFormatter(format).Format(obj);
		}
		public static string SequenceFormat(this string format, params object[] args)
		{
			return new RuntimeFormatter(format).SequenceFormat(args);
		}
	}
}
