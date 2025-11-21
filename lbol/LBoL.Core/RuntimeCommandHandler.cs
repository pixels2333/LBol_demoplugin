using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace LBoL.Core
{
	// Token: 0x02000067 RID: 103
	public sealed class RuntimeCommandHandler
	{
		// Token: 0x06000459 RID: 1113 RVA: 0x0000EC5C File Offset: 0x0000CE5C
		private static List<string> SplitCommandText(string input)
		{
			StringReader stringReader = new StringReader(input.Normalize());
			List<string> list = new List<string>();
			StringBuilder stringBuilder = new StringBuilder();
			int num;
			while ((num = stringReader.Read()) != -1)
			{
				if (char.IsWhiteSpace((char)num))
				{
					if (stringBuilder.Length > 0)
					{
						list.Add(stringBuilder.ToString());
						stringBuilder.Clear();
					}
				}
				else if (num == 34)
				{
					for (;;)
					{
						num = stringReader.Read();
						if (num == -1)
						{
							break;
						}
						if (num == 92)
						{
							int num2 = stringReader.Peek();
							if (num2 == 92 || num2 == 34)
							{
								stringReader.Read();
								stringBuilder.Append((char)num2);
							}
							else
							{
								stringBuilder.Append((char)num);
							}
						}
						else
						{
							if (num == 34)
							{
								goto IL_00AF;
							}
							stringBuilder.Append((char)num);
						}
					}
					list.Add(stringBuilder.ToString());
					return list;
					IL_00AF:
					list.Add(stringBuilder.ToString());
					stringBuilder.Clear();
				}
				else
				{
					stringBuilder.Append((char)num);
				}
			}
			if (stringBuilder.Length > 0)
			{
				list.Add(stringBuilder.ToString());
			}
			return list;
		}

		// Token: 0x0600045A RID: 1114 RVA: 0x0000ED5C File Offset: 0x0000CF5C
		public bool TryHandleCommand(string command, out IEnumerator result)
		{
			result = null;
			List<string> list = RuntimeCommandHandler.SplitCommandText(command);
			if (list.Count < 1)
			{
				return false;
			}
			RuntimeCommandHandler.Handler handler;
			if (!this._handlerTable.TryGetValue(list[0], ref handler))
			{
				return false;
			}
			bool flag;
			try
			{
				object[] array = Enumerable.ToArray<object>(Enumerable.Cast<object>(Enumerable.Skip<string>(list, 1)));
				flag = handler.Handle(array, out result);
			}
			catch (TargetInvocationException ex)
			{
				Debug.LogError("Run command \"" + command + "\" failed in handler: " + ex.Message);
				Debug.LogException(ex.InnerException);
				flag = true;
			}
			catch (Exception ex2)
			{
				Debug.LogError(string.Format("Run command \"{0}\" failed: {1}\n{2}", command, ex2.Message, ex2));
				Debug.LogException(ex2);
				flag = false;
			}
			return flag;
		}

		// Token: 0x0600045B RID: 1115 RVA: 0x0000EE28 File Offset: 0x0000D028
		private void Register(Type type, BindingFlags bindingFlags, RuntimeCommandHandler.MethodInvoker invoker)
		{
			foreach (MethodInfo methodInfo in type.GetMethods(bindingFlags))
			{
				RuntimeCommandAttribute customAttribute = CustomAttributeExtensions.GetCustomAttribute<RuntimeCommandAttribute>(methodInfo);
				if (customAttribute != null)
				{
					string text = customAttribute.Name ?? methodInfo.Name;
					if (this._handlerTable.ContainsKey(text))
					{
						throw new ArgumentException("Duplicated command handler " + text);
					}
					this._handlerTable.Add(text, new RuntimeCommandHandler.Handler(text, customAttribute.Description, type, methodInfo, invoker));
				}
			}
		}

		// Token: 0x0600045C RID: 1116 RVA: 0x0000EEA9 File Offset: 0x0000D0A9
		public void Register<T>()
		{
			this.Register(typeof(T));
		}

		// Token: 0x0600045D RID: 1117 RVA: 0x0000EEBB File Offset: 0x0000D0BB
		public void Register(Type type)
		{
			this.Register(type, 56, (MethodInfo method, object[] args) => method.Invoke(null, args));
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x0000EEE8 File Offset: 0x0000D0E8
		public void Register(object handlerSource)
		{
			this.Register(handlerSource.GetType(), 52, (MethodInfo method, object[] args) => method.Invoke(handlerSource, args));
		}

		// Token: 0x0600045F RID: 1119 RVA: 0x0000EF21 File Offset: 0x0000D121
		public static RuntimeCommandHandler Create<T>()
		{
			RuntimeCommandHandler runtimeCommandHandler = new RuntimeCommandHandler();
			runtimeCommandHandler.Register<T>();
			return runtimeCommandHandler;
		}

		// Token: 0x06000460 RID: 1120 RVA: 0x0000EF2E File Offset: 0x0000D12E
		public static RuntimeCommandHandler Create(Type type)
		{
			RuntimeCommandHandler runtimeCommandHandler = new RuntimeCommandHandler();
			runtimeCommandHandler.Register(type);
			return runtimeCommandHandler;
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x0000EF3C File Offset: 0x0000D13C
		public static RuntimeCommandHandler Merge(RuntimeCommandHandler handler1, RuntimeCommandHandler handler2)
		{
			RuntimeCommandHandler runtimeCommandHandler = new RuntimeCommandHandler();
			string text;
			RuntimeCommandHandler.Handler handler3;
			foreach (KeyValuePair<string, RuntimeCommandHandler.Handler> keyValuePair in handler1._handlerTable)
			{
				keyValuePair.Deconstruct(ref text, ref handler3);
				string text2 = text;
				RuntimeCommandHandler.Handler handler4 = handler3;
				runtimeCommandHandler._handlerTable.TryAdd(text2, handler4);
			}
			foreach (KeyValuePair<string, RuntimeCommandHandler.Handler> keyValuePair in handler2._handlerTable)
			{
				keyValuePair.Deconstruct(ref text, ref handler3);
				string text3 = text;
				RuntimeCommandHandler.Handler handler5 = handler3;
				runtimeCommandHandler._handlerTable.TryAdd(text3, handler5);
			}
			return runtimeCommandHandler;
		}

		// Token: 0x06000462 RID: 1122 RVA: 0x0000F010 File Offset: 0x0000D210
		public static RuntimeCommandHandler Create(object handlerInstance)
		{
			RuntimeCommandHandler runtimeCommandHandler = new RuntimeCommandHandler();
			runtimeCommandHandler.Register(handlerInstance);
			return runtimeCommandHandler;
		}

		// Token: 0x06000463 RID: 1123 RVA: 0x0000F01E File Offset: 0x0000D21E
		[return: TupleElementNames(new string[] { "name", "hint", "description" })]
		public IEnumerable<ValueTuple<string, string, string>> EnumerateCommands()
		{
			foreach (KeyValuePair<string, RuntimeCommandHandler.Handler> keyValuePair in this._handlerTable)
			{
				string text;
				RuntimeCommandHandler.Handler handler;
				keyValuePair.Deconstruct(ref text, ref handler);
				RuntimeCommandHandler.Handler handler2 = handler;
				yield return new ValueTuple<string, string, string>(handler2.Name, handler2.Hint, handler2.Description);
			}
			Dictionary<string, RuntimeCommandHandler.Handler>.Enumerator enumerator = default(Dictionary<string, RuntimeCommandHandler.Handler>.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x0400025B RID: 603
		private readonly Dictionary<string, RuntimeCommandHandler.Handler> _handlerTable = new Dictionary<string, RuntimeCommandHandler.Handler>();

		// Token: 0x02000209 RID: 521
		private abstract class ArgumentConverter
		{
			// Token: 0x06001100 RID: 4352 RVA: 0x0002DF40 File Offset: 0x0002C140
			public static RuntimeCommandHandler.ArgumentConverter GetConverter(Type type)
			{
				RuntimeCommandHandler.ArgumentConverter argumentConverter;
				if (RuntimeCommandHandler.ArgumentConverter.ConverterTable.TryGetValue(type, ref argumentConverter))
				{
					return argumentConverter;
				}
				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable))
				{
					argumentConverter = new RuntimeCommandHandler.ArgumentConverter.NullableConverter(RuntimeCommandHandler.ArgumentConverter.GetConverter(type.GetGenericArguments()[0]));
				}
				else
				{
					RuntimeCommandHandler.ArgumentConverter argumentConverter2;
					switch (Type.GetTypeCode(type))
					{
					case 0:
						throw new ArgumentNullException();
					case 3:
						argumentConverter2 = new RuntimeCommandHandler.ArgumentConverter.BooleanConverter();
						goto IL_012F;
					case 4:
						argumentConverter2 = new RuntimeCommandHandler.ArgumentConverter.CharConverter();
						goto IL_012F;
					case 5:
						argumentConverter2 = new RuntimeCommandHandler.ArgumentConverter.SByteConverter();
						goto IL_012F;
					case 6:
						argumentConverter2 = new RuntimeCommandHandler.ArgumentConverter.ByteConverter();
						goto IL_012F;
					case 7:
						argumentConverter2 = new RuntimeCommandHandler.ArgumentConverter.Int16Converter();
						goto IL_012F;
					case 8:
						argumentConverter2 = new RuntimeCommandHandler.ArgumentConverter.UInt16Converter();
						goto IL_012F;
					case 9:
						argumentConverter2 = new RuntimeCommandHandler.ArgumentConverter.Int32Converter();
						goto IL_012F;
					case 10:
						argumentConverter2 = new RuntimeCommandHandler.ArgumentConverter.UInt32Converter();
						goto IL_012F;
					case 11:
						argumentConverter2 = new RuntimeCommandHandler.ArgumentConverter.Int64Converter();
						goto IL_012F;
					case 12:
						argumentConverter2 = new RuntimeCommandHandler.ArgumentConverter.UInt64Converter();
						goto IL_012F;
					case 13:
						argumentConverter2 = new RuntimeCommandHandler.ArgumentConverter.SingleConverter();
						goto IL_012F;
					case 14:
						argumentConverter2 = new RuntimeCommandHandler.ArgumentConverter.DoubleConverter();
						goto IL_012F;
					case 15:
						argumentConverter2 = new RuntimeCommandHandler.ArgumentConverter.DecimalConverter();
						goto IL_012F;
					case 18:
						argumentConverter2 = new RuntimeCommandHandler.ArgumentConverter.StringConverter();
						goto IL_012F;
					}
					throw new NotSupportedException(type.FullName + " is not supported in commands");
					IL_012F:
					argumentConverter = argumentConverter2;
				}
				RuntimeCommandHandler.ArgumentConverter.ConverterTable.Add(type, argumentConverter);
				return argumentConverter;
			}

			// Token: 0x1700057D RID: 1405
			// (get) Token: 0x06001101 RID: 4353
			public abstract string TypeName { get; }

			// Token: 0x06001102 RID: 4354
			public abstract object ConvertString(string s);

			// Token: 0x040007FC RID: 2044
			private static readonly Dictionary<Type, RuntimeCommandHandler.ArgumentConverter> ConverterTable = new Dictionary<Type, RuntimeCommandHandler.ArgumentConverter>();

			// Token: 0x02000337 RID: 823
			private class NullableConverter : RuntimeCommandHandler.ArgumentConverter
			{
				// Token: 0x0600173F RID: 5951 RVA: 0x00041EFD File Offset: 0x000400FD
				public NullableConverter(RuntimeCommandHandler.ArgumentConverter innerInnerConverter)
				{
					this._innerConverter = innerInnerConverter;
				}

				// Token: 0x170006AE RID: 1710
				// (get) Token: 0x06001740 RID: 5952 RVA: 0x00041F0C File Offset: 0x0004010C
				public override string TypeName
				{
					get
					{
						return this._innerConverter.TypeName + "?";
					}
				}

				// Token: 0x06001741 RID: 5953 RVA: 0x00041F23 File Offset: 0x00040123
				public override object ConvertString(string s)
				{
					if (!string.Equals(s, "null", 5))
					{
						return this._innerConverter.ConvertString(s);
					}
					return null;
				}

				// Token: 0x04000C9F RID: 3231
				private readonly RuntimeCommandHandler.ArgumentConverter _innerConverter;
			}

			// Token: 0x02000338 RID: 824
			private class BooleanConverter : RuntimeCommandHandler.ArgumentConverter
			{
				// Token: 0x170006AF RID: 1711
				// (get) Token: 0x06001742 RID: 5954 RVA: 0x00041F41 File Offset: 0x00040141
				public override string TypeName
				{
					get
					{
						return "bool";
					}
				}

				// Token: 0x06001743 RID: 5955 RVA: 0x00041F48 File Offset: 0x00040148
				public override object ConvertString(string s)
				{
					return bool.Parse(s);
				}
			}

			// Token: 0x02000339 RID: 825
			private class CharConverter : RuntimeCommandHandler.ArgumentConverter
			{
				// Token: 0x170006B0 RID: 1712
				// (get) Token: 0x06001745 RID: 5957 RVA: 0x00041F5D File Offset: 0x0004015D
				public override string TypeName
				{
					get
					{
						return "char";
					}
				}

				// Token: 0x06001746 RID: 5958 RVA: 0x00041F64 File Offset: 0x00040164
				public override object ConvertString(string s)
				{
					return char.Parse(s);
				}
			}

			// Token: 0x0200033A RID: 826
			private class SByteConverter : RuntimeCommandHandler.ArgumentConverter
			{
				// Token: 0x170006B1 RID: 1713
				// (get) Token: 0x06001748 RID: 5960 RVA: 0x00041F79 File Offset: 0x00040179
				public override string TypeName
				{
					get
					{
						return "sbyte";
					}
				}

				// Token: 0x06001749 RID: 5961 RVA: 0x00041F80 File Offset: 0x00040180
				public override object ConvertString(string s)
				{
					return sbyte.Parse(s, 7, CultureInfo.InvariantCulture);
				}
			}

			// Token: 0x0200033B RID: 827
			private class ByteConverter : RuntimeCommandHandler.ArgumentConverter
			{
				// Token: 0x170006B2 RID: 1714
				// (get) Token: 0x0600174B RID: 5963 RVA: 0x00041F9B File Offset: 0x0004019B
				public override string TypeName
				{
					get
					{
						return "byte";
					}
				}

				// Token: 0x0600174C RID: 5964 RVA: 0x00041FA2 File Offset: 0x000401A2
				public override object ConvertString(string s)
				{
					return byte.Parse(s);
				}
			}

			// Token: 0x0200033C RID: 828
			private class Int16Converter : RuntimeCommandHandler.ArgumentConverter
			{
				// Token: 0x170006B3 RID: 1715
				// (get) Token: 0x0600174E RID: 5966 RVA: 0x00041FB7 File Offset: 0x000401B7
				public override string TypeName
				{
					get
					{
						return "short";
					}
				}

				// Token: 0x0600174F RID: 5967 RVA: 0x00041FBE File Offset: 0x000401BE
				public override object ConvertString(string s)
				{
					return short.Parse(s, 7, CultureInfo.InvariantCulture);
				}
			}

			// Token: 0x0200033D RID: 829
			private class UInt16Converter : RuntimeCommandHandler.ArgumentConverter
			{
				// Token: 0x170006B4 RID: 1716
				// (get) Token: 0x06001751 RID: 5969 RVA: 0x00041FD9 File Offset: 0x000401D9
				public override string TypeName
				{
					get
					{
						return "ushort";
					}
				}

				// Token: 0x06001752 RID: 5970 RVA: 0x00041FE0 File Offset: 0x000401E0
				public override object ConvertString(string s)
				{
					return ushort.Parse(s, 7, CultureInfo.InvariantCulture);
				}
			}

			// Token: 0x0200033E RID: 830
			private class Int32Converter : RuntimeCommandHandler.ArgumentConverter
			{
				// Token: 0x170006B5 RID: 1717
				// (get) Token: 0x06001754 RID: 5972 RVA: 0x00041FFB File Offset: 0x000401FB
				public override string TypeName
				{
					get
					{
						return "int";
					}
				}

				// Token: 0x06001755 RID: 5973 RVA: 0x00042002 File Offset: 0x00040202
				public override object ConvertString(string s)
				{
					return int.Parse(s, 7, CultureInfo.InvariantCulture);
				}
			}

			// Token: 0x0200033F RID: 831
			private class UInt32Converter : RuntimeCommandHandler.ArgumentConverter
			{
				// Token: 0x170006B6 RID: 1718
				// (get) Token: 0x06001757 RID: 5975 RVA: 0x0004201D File Offset: 0x0004021D
				public override string TypeName
				{
					get
					{
						return "uint";
					}
				}

				// Token: 0x06001758 RID: 5976 RVA: 0x00042024 File Offset: 0x00040224
				public override object ConvertString(string s)
				{
					return uint.Parse(s, 7, CultureInfo.InvariantCulture);
				}
			}

			// Token: 0x02000340 RID: 832
			private class Int64Converter : RuntimeCommandHandler.ArgumentConverter
			{
				// Token: 0x170006B7 RID: 1719
				// (get) Token: 0x0600175A RID: 5978 RVA: 0x0004203F File Offset: 0x0004023F
				public override string TypeName
				{
					get
					{
						return "long";
					}
				}

				// Token: 0x0600175B RID: 5979 RVA: 0x00042046 File Offset: 0x00040246
				public override object ConvertString(string s)
				{
					return long.Parse(s, 7, CultureInfo.InvariantCulture);
				}
			}

			// Token: 0x02000341 RID: 833
			private class UInt64Converter : RuntimeCommandHandler.ArgumentConverter
			{
				// Token: 0x170006B8 RID: 1720
				// (get) Token: 0x0600175D RID: 5981 RVA: 0x00042061 File Offset: 0x00040261
				public override string TypeName
				{
					get
					{
						return "ulong";
					}
				}

				// Token: 0x0600175E RID: 5982 RVA: 0x00042068 File Offset: 0x00040268
				public override object ConvertString(string s)
				{
					return ulong.Parse(s, 7, CultureInfo.InvariantCulture);
				}
			}

			// Token: 0x02000342 RID: 834
			private class DecimalConverter : RuntimeCommandHandler.ArgumentConverter
			{
				// Token: 0x170006B9 RID: 1721
				// (get) Token: 0x06001760 RID: 5984 RVA: 0x00042083 File Offset: 0x00040283
				public override string TypeName
				{
					get
					{
						return "decimal";
					}
				}

				// Token: 0x06001761 RID: 5985 RVA: 0x0004208A File Offset: 0x0004028A
				public override object ConvertString(string s)
				{
					return decimal.Parse(s, 167, CultureInfo.InvariantCulture);
				}
			}

			// Token: 0x02000343 RID: 835
			private class SingleConverter : RuntimeCommandHandler.ArgumentConverter
			{
				// Token: 0x170006BA RID: 1722
				// (get) Token: 0x06001763 RID: 5987 RVA: 0x000420A9 File Offset: 0x000402A9
				public override string TypeName
				{
					get
					{
						return "float";
					}
				}

				// Token: 0x06001764 RID: 5988 RVA: 0x000420B0 File Offset: 0x000402B0
				public override object ConvertString(string s)
				{
					return float.Parse(s, 167, CultureInfo.InvariantCulture);
				}
			}

			// Token: 0x02000344 RID: 836
			private class DoubleConverter : RuntimeCommandHandler.ArgumentConverter
			{
				// Token: 0x170006BB RID: 1723
				// (get) Token: 0x06001766 RID: 5990 RVA: 0x000420CF File Offset: 0x000402CF
				public override string TypeName
				{
					get
					{
						return "double";
					}
				}

				// Token: 0x06001767 RID: 5991 RVA: 0x000420D6 File Offset: 0x000402D6
				public override object ConvertString(string s)
				{
					return double.Parse(s, 167, CultureInfo.InvariantCulture);
				}
			}

			// Token: 0x02000345 RID: 837
			private class StringConverter : RuntimeCommandHandler.ArgumentConverter
			{
				// Token: 0x170006BC RID: 1724
				// (get) Token: 0x06001769 RID: 5993 RVA: 0x000420F5 File Offset: 0x000402F5
				public override string TypeName
				{
					get
					{
						return "string";
					}
				}

				// Token: 0x0600176A RID: 5994 RVA: 0x000420FC File Offset: 0x000402FC
				public override object ConvertString(string s)
				{
					return s;
				}
			}
		}

		// Token: 0x0200020A RID: 522
		private sealed class ArgumentHandler
		{
			// Token: 0x1700057E RID: 1406
			// (get) Token: 0x06001105 RID: 4357 RVA: 0x0002E09F File Offset: 0x0002C29F
			public string Name { get; }

			// Token: 0x1700057F RID: 1407
			// (get) Token: 0x06001106 RID: 4358 RVA: 0x0002E0A7 File Offset: 0x0002C2A7
			public bool HasDefaultValue { get; }

			// Token: 0x17000580 RID: 1408
			// (get) Token: 0x06001107 RID: 4359 RVA: 0x0002E0AF File Offset: 0x0002C2AF
			public Type Type { get; }

			// Token: 0x17000581 RID: 1409
			// (get) Token: 0x06001108 RID: 4360 RVA: 0x0002E0B7 File Offset: 0x0002C2B7
			public bool IsParams { get; }

			// Token: 0x17000582 RID: 1410
			// (get) Token: 0x06001109 RID: 4361 RVA: 0x0002E0BF File Offset: 0x0002C2BF
			public string TypeName
			{
				get
				{
					return this._converter.TypeName;
				}
			}

			// Token: 0x0600110A RID: 4362 RVA: 0x0002E0CC File Offset: 0x0002C2CC
			public ArgumentHandler(string name, Type type, bool isParam)
			{
				this.Name = name;
				this.Type = type;
				this.IsParams = isParam;
				this._converter = RuntimeCommandHandler.ArgumentConverter.GetConverter(type);
				this.HasDefaultValue = false;
			}

			// Token: 0x0600110B RID: 4363 RVA: 0x0002E0FC File Offset: 0x0002C2FC
			public ArgumentHandler(string name, Type type, bool isParam, object defaultValue)
			{
				this.Name = name;
				this.Type = type;
				this.IsParams = isParam;
				this._converter = RuntimeCommandHandler.ArgumentConverter.GetConverter(type);
				this._defaultValue = defaultValue;
				this.HasDefaultValue = true;
			}

			// Token: 0x0600110C RID: 4364 RVA: 0x0002E134 File Offset: 0x0002C334
			public object Convert([CanBeNull] object value)
			{
				if (value != null)
				{
					string text = value as string;
					if (text == null)
					{
						throw new ArgumentOutOfRangeException("value", value, null);
					}
					return this._converter.ConvertString(text);
				}
				else
				{
					if (!this.HasDefaultValue)
					{
						throw new ArgumentException("Parameter <" + this.Name + "> value not provided and has no default value");
					}
					return this._defaultValue;
				}
			}

			// Token: 0x04000801 RID: 2049
			private readonly RuntimeCommandHandler.ArgumentConverter _converter;

			// Token: 0x04000802 RID: 2050
			private readonly object _defaultValue;
		}

		// Token: 0x0200020B RID: 523
		private sealed class Handler
		{
			// Token: 0x17000583 RID: 1411
			// (get) Token: 0x0600110D RID: 4365 RVA: 0x0002E191 File Offset: 0x0002C391
			public string Name { get; }

			// Token: 0x17000584 RID: 1412
			// (get) Token: 0x0600110E RID: 4366 RVA: 0x0002E199 File Offset: 0x0002C399
			public string Description { get; }

			// Token: 0x17000585 RID: 1413
			// (get) Token: 0x0600110F RID: 4367 RVA: 0x0002E1A1 File Offset: 0x0002C3A1
			public string Hint { get; }

			// Token: 0x06001110 RID: 4368 RVA: 0x0002E1AC File Offset: 0x0002C3AC
			public Handler(string name, string description, Type type, MethodInfo method, RuntimeCommandHandler.MethodInvoker invoker)
			{
				Type returnType = method.ReturnType;
				if (method.IsGenericMethod)
				{
					throw new ArgumentException(type.FullName + method.Name + " is generic");
				}
				if (returnType != typeof(void) && returnType != typeof(IEnumerator))
				{
					throw new ArgumentException(string.Format("Return type of {0}{1} is neither void nor IEnumerator: {2}", type.FullName, method.Name, returnType));
				}
				foreach (ParameterInfo parameterInfo in method.GetParameters())
				{
					RuntimeCommandHandler.ArgumentHandler argumentHandler;
					if (CustomAttributeExtensions.GetCustomAttribute<ParamArrayAttribute>(parameterInfo) != null)
					{
						Type elementType = parameterInfo.ParameterType.GetElementType();
						argumentHandler = new RuntimeCommandHandler.ArgumentHandler(parameterInfo.Name, elementType, true);
					}
					else if (parameterInfo.HasDefaultValue)
					{
						argumentHandler = new RuntimeCommandHandler.ArgumentHandler(parameterInfo.Name, parameterInfo.ParameterType, false, parameterInfo.DefaultValue);
					}
					else
					{
						argumentHandler = new RuntimeCommandHandler.ArgumentHandler(parameterInfo.Name, parameterInfo.ParameterType, false);
					}
					this._argHandlers.Add(argumentHandler);
				}
				this.Name = name;
				this.Description = description;
				StringBuilder stringBuilder = new StringBuilder(name);
				foreach (RuntimeCommandHandler.ArgumentHandler argumentHandler2 in this._argHandlers)
				{
					if (argumentHandler2.HasDefaultValue)
					{
						stringBuilder.Append(" [").Append(argumentHandler2.Name).Append(": ")
							.Append(argumentHandler2.TypeName)
							.Append(']');
					}
					else if (argumentHandler2.IsParams)
					{
						stringBuilder.Append(" {").Append(argumentHandler2.Name).Append(": ")
							.Append(argumentHandler2.TypeName)
							.Append("}");
					}
					else
					{
						stringBuilder.Append(' ').Append(argumentHandler2.Name).Append(": ")
							.Append(argumentHandler2.TypeName);
					}
				}
				this.Hint = stringBuilder.ToString();
				this._method = method;
				this._invoker = invoker;
			}

			// Token: 0x06001111 RID: 4369 RVA: 0x0002E3F4 File Offset: 0x0002C5F4
			public bool Handle(IReadOnlyList<object> args, out IEnumerator result)
			{
				bool flag = this._argHandlers.Count > 0 && Enumerable.Last<RuntimeCommandHandler.ArgumentHandler>(this._argHandlers).IsParams;
				if (args.Count > this._argHandlers.Count && !flag)
				{
					throw new ArgumentException("Too many argument passed to command " + this.Name);
				}
				object[] array = new object[this._argHandlers.Count];
				if (flag)
				{
					for (int i = 0; i < this._argHandlers.Count - 1; i++)
					{
						array[i] = this._argHandlers[i].Convert((i < args.Count) ? args[i] : null);
					}
					RuntimeCommandHandler.ArgumentHandler argumentHandler = Enumerable.Last<RuntimeCommandHandler.ArgumentHandler>(this._argHandlers);
					int num = args.Count - this._argHandlers.Count + 1;
					Array array2 = Array.CreateInstance(argumentHandler.Type, num);
					int j = 0;
					int num2 = this._argHandlers.Count - 1;
					while (j < num)
					{
						array2.SetValue(argumentHandler.Convert(args[num2]), j);
						j++;
						num2++;
					}
					array[this._argHandlers.Count - 1] = array2;
				}
				else
				{
					for (int k = 0; k < this._argHandlers.Count; k++)
					{
						array[k] = this._argHandlers[k].Convert((k < args.Count) ? args[k] : null);
					}
				}
				object obj = this._invoker(this._method, array);
				result = (IEnumerator)obj;
				return true;
			}

			// Token: 0x04000806 RID: 2054
			private readonly MethodInfo _method;

			// Token: 0x04000807 RID: 2055
			private readonly RuntimeCommandHandler.MethodInvoker _invoker;

			// Token: 0x04000808 RID: 2056
			private readonly List<RuntimeCommandHandler.ArgumentHandler> _argHandlers = new List<RuntimeCommandHandler.ArgumentHandler>();
		}

		// Token: 0x0200020C RID: 524
		// (Invoke) Token: 0x06001113 RID: 4371
		private delegate object MethodInvoker(MethodInfo method, object[] args);
	}
}
