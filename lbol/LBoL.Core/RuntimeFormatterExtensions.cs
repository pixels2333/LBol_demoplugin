using System;

namespace LBoL.Core
{
	// Token: 0x0200006A RID: 106
	public static class RuntimeFormatterExtensions
	{
		// Token: 0x06000476 RID: 1142 RVA: 0x0000F983 File Offset: 0x0000DB83
		public static string RuntimeFormat(this string format, RuntimeFormatterArgmentHandler argumentHandler)
		{
			return new RuntimeFormatter(format).Format(argumentHandler);
		}

		// Token: 0x06000477 RID: 1143 RVA: 0x0000F994 File Offset: 0x0000DB94
		public static string RuntimeFormat(this string format, Func<string, string> valueGetter)
		{
			return new RuntimeFormatter(format).Format((string key, string _) => valueGetter.Invoke(key));
		}

		// Token: 0x06000478 RID: 1144 RVA: 0x0000F9C5 File Offset: 0x0000DBC5
		internal static string RuntimeFormat(this string format, GameEntityFormatWrapper formatWrapper)
		{
			return new RuntimeFormatter(format).Format(formatWrapper);
		}

		// Token: 0x06000479 RID: 1145 RVA: 0x0000F9D3 File Offset: 0x0000DBD3
		public static string RuntimeFormat(this string format, object obj)
		{
			return new RuntimeFormatter(format).Format(obj);
		}

		// Token: 0x0600047A RID: 1146 RVA: 0x0000F9E1 File Offset: 0x0000DBE1
		public static string SequenceFormat(this string format, params object[] args)
		{
			return new RuntimeFormatter(format).SequenceFormat(args);
		}
	}
}
