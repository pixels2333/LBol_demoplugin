using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
namespace LBoL.Core
{
	public sealed class RuntimeFormatter
	{
		public RuntimeFormatter([NotNull] string format)
		{
			int num = 0;
			int length = format.Length;
			StringBuilder stringBuilder = new StringBuilder();
			for (;;)
			{
				char c;
				if (num < length)
				{
					c = format.get_Chars(num);
					num++;
					if (c == '}')
					{
						if (num >= length || format.get_Chars(num) != '}')
						{
							break;
						}
						num++;
					}
					if (c == '{')
					{
						if (num >= length || format.get_Chars(num) != '{')
						{
							this._segments.Add(new RuntimeFormatter.StringSegment(stringBuilder.ToString()));
							stringBuilder = new StringBuilder();
							num--;
							goto IL_0097;
						}
						num++;
					}
					stringBuilder.Append(c);
					continue;
				}
				IL_0097:
				if (num == length)
				{
					goto Block_7;
				}
				num++;
				if (num == length)
				{
					goto Block_9;
				}
				c = format.get_Chars(num);
				if (c == ',' || c == ':' || c == '}')
				{
					goto IL_00E4;
				}
				StringBuilder stringBuilder2 = new StringBuilder();
				do
				{
					stringBuilder2.Append(c);
					num++;
					if (num == length)
					{
						goto Block_12;
					}
					c = format.get_Chars(num);
					if (c == ',' || c == ':')
					{
						break;
					}
				}
				while (c != '}');
				IL_0127:
				while (num < length && (c = format.get_Chars(num)) == ' ')
				{
					num++;
				}
				bool flag = false;
				int num2 = 0;
				if (c == ',')
				{
					num++;
					while (num < length && format.get_Chars(num) == ' ')
					{
						num++;
					}
					if (num == length)
					{
						goto Block_19;
					}
					c = format.get_Chars(num);
					if (c == '-')
					{
						flag = true;
						num++;
						if (num == length)
						{
							goto Block_21;
						}
						c = format.get_Chars(num);
					}
					if (c < '0' || c > '9')
					{
						goto IL_019B;
					}
					do
					{
						num2 = num2 * 10 + (int)c - 48;
						num++;
						if (num == length)
						{
							goto Block_23;
						}
						c = format.get_Chars(num);
						if (c < '0' || c > '9')
						{
							break;
						}
					}
					while (num2 < 1000000);
				}
				while (num < length && (c = format.get_Chars(num)) == ' ')
				{
					num++;
				}
				StringBuilder stringBuilder3 = null;
				if (c == ':')
				{
					num++;
					while (num != length)
					{
						c = format.get_Chars(num);
						num++;
						if (c == '{')
						{
							if (num >= length || format.get_Chars(num) != '{')
							{
								goto IL_022C;
							}
							num++;
						}
						else if (c == '}')
						{
							if (num >= length || format.get_Chars(num) != '}')
							{
								num--;
								goto IL_0269;
							}
							num++;
						}
						if (stringBuilder3 == null)
						{
							stringBuilder3 = new StringBuilder();
						}
						stringBuilder3.Append(c);
					}
					goto Block_29;
				}
				IL_0269:
				if (c != '}')
				{
					goto Block_37;
				}
				num++;
				this._segments.Add(new RuntimeFormatter.ObjectSegment(stringBuilder2.ToString(), num2, flag, ((stringBuilder3 != null) ? stringBuilder3.ToString() : null) ?? string.Empty));
				continue;
				goto IL_0127;
			}
			throw RuntimeFormatter.FormatError(format);
			Block_7:
			if (stringBuilder.Length > 0)
			{
				this._segments.Add(new RuntimeFormatter.StringSegment(stringBuilder.ToString()));
				return;
			}
			return;
			Block_9:
			throw RuntimeFormatter.FormatError(format);
			IL_00E4:
			throw RuntimeFormatter.FormatError(format);
			Block_12:
			throw RuntimeFormatter.FormatError(format);
			Block_19:
			throw RuntimeFormatter.FormatError(format);
			Block_21:
			throw RuntimeFormatter.FormatError(format);
			IL_019B:
			throw RuntimeFormatter.FormatError(format);
			Block_23:
			throw RuntimeFormatter.FormatError(format);
			Block_29:
			throw RuntimeFormatter.FormatError(format);
			IL_022C:
			throw RuntimeFormatter.FormatError(format);
			Block_37:
			throw RuntimeFormatter.FormatError(format);
		}
		private static FormatException FormatError(string content)
		{
			return new FormatException("Invalid format string: " + content);
		}
		public string Format(RuntimeFormatterArgmentHandler argumentHandler)
		{
			if (this._segments.Count == 0)
			{
				return string.Empty;
			}
			if (this._segments.Count == 1)
			{
				return this._segments[0].Shoot(argumentHandler);
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (RuntimeFormatter.Segment segment in this._segments)
			{
				segment.Shoot(stringBuilder, argumentHandler);
			}
			return Localization.Humanize(stringBuilder.ToString());
		}
		internal string Format(GameEntityFormatWrapper formatWrapper)
		{
			return this.Format(new RuntimeFormatterArgmentHandler(formatWrapper.Format));
		}
		public string Format(object obj)
		{
			Type type = obj.GetType();
			return this.Format(delegate(string key, string format)
			{
				PropertyInfo property = type.GetProperty(key);
				if (property != null)
				{
					return RuntimeFormatter.FormatArgument(property.GetValue(obj), format);
				}
				FieldInfo field = type.GetField(key);
				if (field != null)
				{
					return RuntimeFormatter.FormatArgument(field.GetValue(obj), format);
				}
				Debug.LogError(string.Concat(new string[]
				{
					"Property '",
					key,
					"' not found when formatting '",
					obj.GetType().Name,
					"' with key '",
					key,
					"'"
				}));
				return string.Empty;
			});
		}
		public string SequenceFormat(params object[] args)
		{
			return this.Format(delegate(string key, string format)
			{
				int num;
				if (!int.TryParse(key, ref num))
				{
					throw new ArgumentException("Invalid sequence format string");
				}
				if (num < 0 || num >= args.Length)
				{
					string[] array = new string[5];
					array[0] = "Element '";
					array[1] = key;
					array[2] = "' not found when formatting with args [";
					array[3] = string.Join(", ", Enumerable.Select<object, string>(args, (object o) => o.ToString()));
					array[4] = "]'";
					throw new ArgumentException(string.Concat(array));
				}
				return RuntimeFormatter.FormatArgument(args[num], format);
			});
		}
		internal static string FormatArgument(object arg, string format)
		{
			if (arg == null)
			{
				return string.Empty;
			}
			if (format == null)
			{
				return arg.ToString() ?? string.Empty;
			}
			string text;
			if (RuntimeFormatter.TryGetSpecialSubstitude(arg, format, out text))
			{
				return RuntimeFormatter.ApplySubstitude(arg.ToString(), text);
			}
			IFormattable formattable = arg as IFormattable;
			if (formattable != null)
			{
				return formattable.ToString(format, null);
			}
			return arg.ToString() ?? string.Empty;
		}
		private unsafe static bool TryGetSubstitude(ReadOnlySpan<char> format, ReadOnlySpan<char> finding, [NotNullWhen(true)] out string output)
		{
			ReadOnlySpan<char> readOnlySpan;
			int num2;
			for (;;)
			{
				format = MemoryExtensions.TrimStart(format);
				if (format.Length == 0)
				{
					break;
				}
				if (MemoryExtensions.StartsWith(format, finding, 5))
				{
					goto Block_1;
				}
				int num = 0;
				while (num < format.Length && *format[num] != 61)
				{
					num++;
				}
				if (num == format.Length)
				{
					goto Block_9;
				}
				num++;
				if (num == format.Length)
				{
					goto Block_10;
				}
				char c;
				if (*format[num] == 34 || *format[num] == 39)
				{
					c = (char)(*format[num]);
					num++;
				}
				else
				{
					c = ' ';
				}
				while (num < format.Length && *format[num] != (ushort)c)
				{
					num++;
				}
				if (num < format.Length && *format[num] == (ushort)c)
				{
					num++;
				}
				readOnlySpan = format;
				num2 = num;
				format = readOnlySpan.Slice(num2, readOnlySpan.Length - num2);
			}
			output = null;
			return false;
			Block_1:
			int num3 = finding.Length;
			if (num3 >= format.Length || *format[num3] != 61)
			{
				throw new ArgumentException("Invalid format " + new string(format));
			}
			num3++;
			if (num3 >= format.Length)
			{
				throw new ArgumentException("Invalid format " + new string(format));
			}
			char c2;
			if (*format[num3] == 34 || *format[num3] == 39)
			{
				c2 = (char)(*format[num3]);
				num3++;
			}
			else
			{
				c2 = ' ';
			}
			int num4 = num3;
			while (num4 < format.Length && *format[num4] != (ushort)c2)
			{
				num4++;
			}
			if (num4 == format.Length && c2 != ' ')
			{
				throw new ArgumentException("Invalid format " + new string(format));
			}
			readOnlySpan = format;
			num2 = num3;
			output = new string(readOnlySpan.Slice(num2, num4 - num2));
			return true;
			Block_9:
			output = null;
			return false;
			Block_10:
			output = null;
			return false;
		}
		private static string GetPluralSubstitude(int num, ReadOnlySpan<char> format)
		{
			if (num < 0)
			{
				throw new ArgumentException(string.Format("Cannot substitude {0} in {1}", num, new string(format)));
			}
			string text = Localization.CardinalRule(num);
			string text2;
			if (text != null && RuntimeFormatter.TryGetSubstitude(format, text, out text2))
			{
				return text2;
			}
			if (RuntimeFormatter.TryGetSubstitude(format, "other", out text2))
			{
				return text2;
			}
			throw new ArgumentException(string.Format("Cannot substitude {0} in '{1}'", num, new string(format)));
		}
		private static string GetOrdinalSubstitude(int num, ReadOnlySpan<char> format)
		{
			if (num < 0)
			{
				throw new ArgumentException(string.Format("Cannot substitude {0} in {1}", num, new string(format)));
			}
			string text = Localization.OrdinalRule(num);
			string text2;
			if (RuntimeFormatter.TryGetSubstitude(format, text ?? "other", out text2))
			{
				return text2;
			}
			throw new ArgumentException(string.Format("Cannot substitude {0} in '{1}'", num, new string(format)));
		}
		internal static string GetSelectSubstitude(string content, ReadOnlySpan<char> format)
		{
			string text;
			if (RuntimeFormatter.TryGetSubstitude(format, content, out text))
			{
				return text;
			}
			throw new ArgumentException(string.Concat(new string[]
			{
				"Cannot substitude ",
				content,
				" in '",
				new string(format),
				"'"
			}));
		}
		internal static bool TryGetSpecialSubstitude(object arg, ReadOnlySpan<char> format, [NotNullWhen(true)] out string substitude)
		{
			if (MemoryExtensions.StartsWith(format, "plural ", 5))
			{
				IConvertible convertible = arg as IConvertible;
				if (convertible == null)
				{
					throw new InvalidOperationException(string.Format("Cannot substitude plural format '{0}' with arg {1}", new string(format), arg));
				}
				int num = convertible.ToInt32(null);
				ReadOnlySpan<char> readOnlySpan = format;
				substitude = RuntimeFormatter.GetPluralSubstitude(num, readOnlySpan.Slice(7, readOnlySpan.Length - 7));
				return true;
			}
			else if (MemoryExtensions.StartsWith(format, "cardinal ", 5))
			{
				IConvertible convertible2 = arg as IConvertible;
				if (convertible2 == null)
				{
					throw new InvalidOperationException(string.Format("Cannot substitude cardinal format '{0}' with arg {1}", new string(format), arg));
				}
				int num2 = convertible2.ToInt32(null);
				ReadOnlySpan<char> readOnlySpan = format;
				substitude = RuntimeFormatter.GetPluralSubstitude(num2, readOnlySpan.Slice(9, readOnlySpan.Length - 9));
				return true;
			}
			else if (MemoryExtensions.StartsWith(format, "ordinal ", 5))
			{
				IConvertible convertible3 = arg as IConvertible;
				if (convertible3 == null)
				{
					throw new InvalidOperationException(string.Format("Cannot substitude ordinal format '{0}' with arg {1}", new string(format), arg));
				}
				int num3 = convertible3.ToInt32(null);
				ReadOnlySpan<char> readOnlySpan = format;
				substitude = RuntimeFormatter.GetOrdinalSubstitude(num3, readOnlySpan.Slice(8, readOnlySpan.Length - 8));
				return true;
			}
			else
			{
				if (!MemoryExtensions.StartsWith(format, "select ", 5))
				{
					substitude = null;
					return false;
				}
				string text = arg.ToString();
				if (string.IsNullOrWhiteSpace(text))
				{
					throw new InvalidOperationException("Cannot substitude select format '" + new string(format) + "' with empty content");
				}
				string text2 = text;
				ReadOnlySpan<char> readOnlySpan = format;
				substitude = RuntimeFormatter.GetSelectSubstitude(text2, readOnlySpan.Slice(7, readOnlySpan.Length - 7));
				return true;
			}
		}
		internal static string ApplySubstitude(string arg, string substitude)
		{
			int num = substitude.IndexOf('%');
			if (num >= 0)
			{
				string text = substitude.Substring(0, num);
				int num2 = num + 1;
				return text + arg + substitude.Substring(num2, substitude.Length - num2);
			}
			return substitude;
		}
		private readonly List<RuntimeFormatter.Segment> _segments = new List<RuntimeFormatter.Segment>();
		private abstract class Segment
		{
			public abstract string Shoot(RuntimeFormatterArgmentHandler handler);
			public abstract void Shoot(StringBuilder builder, RuntimeFormatterArgmentHandler handler);
		}
		private class StringSegment : RuntimeFormatter.Segment
		{
			public StringSegment(string content)
			{
				this._content = content;
			}
			public override string Shoot(RuntimeFormatterArgmentHandler handler)
			{
				return this._content;
			}
			public override void Shoot(StringBuilder builder, RuntimeFormatterArgmentHandler handler)
			{
				builder.Append(this._content);
			}
			private readonly string _content;
		}
		private class ObjectSegment : RuntimeFormatter.Segment
		{
			public ObjectSegment(string key, int width, bool leftJustify, [MaybeNull] string format)
			{
				this._key = key;
				this._width = width;
				this._leftJustify = leftJustify;
				this._format = format;
			}
			public override string Shoot(RuntimeFormatterArgmentHandler handler)
			{
				string text = handler(this._key, this._format) ?? string.Empty;
				int num = this._width - text.Length;
				if (!this._leftJustify)
				{
					return text.PadLeft(num);
				}
				return text.PadRight(num);
			}
			public override void Shoot(StringBuilder builder, RuntimeFormatterArgmentHandler handler)
			{
				string text = handler(this._key, this._format);
				int num = this._width - text.Length;
				if (!this._leftJustify && num > 0)
				{
					builder.Append(' ', num);
				}
				builder.Append(text);
				if (this._leftJustify && num > 0)
				{
					builder.Append(' ', num);
				}
			}
			private readonly string _key;
			private readonly int _width;
			private readonly bool _leftJustify;
			[MaybeNull]
			private readonly string _format;
		}
	}
}
