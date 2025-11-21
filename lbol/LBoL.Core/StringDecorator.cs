using System;
using System.Text;
using UnityEngine;

namespace LBoL.Core
{
	// Token: 0x02000070 RID: 112
	public static class StringDecorator
	{
		// Token: 0x06000512 RID: 1298 RVA: 0x000109F4 File Offset: 0x0000EBF4
		private static int ReadSegment(string source, int i, StringBuilder builder, char end, Color defaultColor)
		{
			int length = source.Length;
			if (i <= length && source.get_Chars(i) == end)
			{
				builder.Append(end);
				return i;
			}
			if (i + 2 <= length && source.get_Chars(i + 1) == ':')
			{
				Color color;
				if (!GlobalConfig.ColorCodeTable.TryGetValue(source.get_Chars(i), ref color))
				{
					color = defaultColor;
					Debug.LogError(string.Format("Cannot find custom color code: {0}", source.get_Chars(i)));
				}
				builder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGBA(color)).Append('>');
				i += 2;
			}
			else
			{
				builder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGBA(defaultColor)).Append('>');
			}
			while (i != length)
			{
				if (source.get_Chars(i) == end)
				{
					builder.Append("</color>");
					return i;
				}
				builder.Append(source.get_Chars(i));
				i++;
			}
			throw new ArgumentException("Invalid decorate source '(" + source.Replace("\n", "\\n") + ")': missing close tag");
		}

		// Token: 0x06000513 RID: 1299 RVA: 0x00010B00 File Offset: 0x0000ED00
		public static string Decorate(string source, Color keywordColor)
		{
			string text;
			try
			{
				int i = 0;
				int length = source.Length;
				StringBuilder stringBuilder = new StringBuilder();
				while (i < length)
				{
					char c = source.get_Chars(i);
					if (c == '|')
					{
						i = StringDecorator.ReadSegment(source, i + 1, stringBuilder, '|', keywordColor);
					}
					else
					{
						stringBuilder.Append(c);
					}
					i++;
				}
				text = stringBuilder.ToString();
			}
			catch (Exception ex)
			{
				Debug.LogError("[StringDecorator] Failed to decorate '" + source + "': " + ex.Message);
				text = "<Error>";
			}
			return text;
		}

		// Token: 0x06000514 RID: 1300 RVA: 0x00010B90 File Offset: 0x0000ED90
		public static string Decorate(string source)
		{
			return StringDecorator.Decorate(source, GlobalConfig.DefaultKeywordColor);
		}

		// Token: 0x06000515 RID: 1301 RVA: 0x00010B9D File Offset: 0x0000ED9D
		public static string GetEntityName(string rawName)
		{
			return string.Concat(new string[]
			{
				"<color=#",
				ColorUtility.ToHtmlStringRGBA(GlobalConfig.EntityColor),
				">",
				rawName,
				"</color>"
			});
		}
	}
}
