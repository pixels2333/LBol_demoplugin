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
	public sealed class RuntimeCommandHandler
	{
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
		public void Register<T>()
		{
			this.Register(typeof(T));
		}
		public void Register(Type type)
		{
			this.Register(type, 56, (MethodInfo method, object[] args) => method.Invoke(null, args));
		}
		public void Register(object handlerSource)
		{
			this.Register(handlerSource.GetType(), 52, (MethodInfo method, object[] args) => method.Invoke(handlerSource, args));
		}
		public static RuntimeCommandHandler Create<T>()
		{
			RuntimeCommandHandler runtimeCommandHandler = new RuntimeCommandHandler();
			runtimeCommandHandler.Register<T>();
			return runtimeCommandHandler;
		}
		public static RuntimeCommandHandler Create(Type type)
		{
			RuntimeCommandHandler runtimeCommandHandler = new RuntimeCommandHandler();
			runtimeCommandHandler.Register(type);
			return runtimeCommandHandler;
		}
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
		public static RuntimeCommandHandler Create(object handlerInstance)
		{
			RuntimeCommandHandler runtimeCommandHandler = new RuntimeCommandHandler();
			runtimeCommandHandler.Register(handlerInstance);
			return runtimeCommandHandler;
		}
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
		private readonly Dictionary<string, RuntimeCommandHandler.Handler> _handlerTable = new Dictionary<string, RuntimeCommandHandler.Handler>();
		private abstract class ArgumentConverter
		{
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
			public abstract string TypeName { get; }
			public abstract object ConvertString(string s);
			private static readonly Dictionary<Type, RuntimeCommandHandler.ArgumentConverter> ConverterTable = new Dictionary<Type, RuntimeCommandHandler.ArgumentConverter>();
			private class NullableConverter : RuntimeCommandHandler.ArgumentConverter
			{
				public NullableConverter(RuntimeCommandHandler.ArgumentConverter innerInnerConverter)
				{
					this._innerConverter = innerInnerConverter;
				}
				public override string TypeName
				{
					get
					{
						return this._innerConverter.TypeName + "?";
					}
				}
				public override object ConvertString(string s)
				{
					if (!string.Equals(s, "null", 5))
					{
						return this._innerConverter.ConvertString(s);
					}
					return null;
				}
				private readonly RuntimeCommandHandler.ArgumentConverter _innerConverter;
			}
			private class BooleanConverter : RuntimeCommandHandler.ArgumentConverter
			{
				public override string TypeName
				{
					get
					{
						return "bool";
					}
				}
				public override object ConvertString(string s)
				{
					return bool.Parse(s);
				}
			}
			private class CharConverter : RuntimeCommandHandler.ArgumentConverter
			{
				public override string TypeName
				{
					get
					{
						return "char";
					}
				}
				public override object ConvertString(string s)
				{
					return char.Parse(s);
				}
			}
			private class SByteConverter : RuntimeCommandHandler.ArgumentConverter
			{
				public override string TypeName
				{
					get
					{
						return "sbyte";
					}
				}
				public override object ConvertString(string s)
				{
					return sbyte.Parse(s, 7, CultureInfo.InvariantCulture);
				}
			}
			private class ByteConverter : RuntimeCommandHandler.ArgumentConverter
			{
				public override string TypeName
				{
					get
					{
						return "byte";
					}
				}
				public override object ConvertString(string s)
				{
					return byte.Parse(s);
				}
			}
			private class Int16Converter : RuntimeCommandHandler.ArgumentConverter
			{
				public override string TypeName
				{
					get
					{
						return "short";
					}
				}
				public override object ConvertString(string s)
				{
					return short.Parse(s, 7, CultureInfo.InvariantCulture);
				}
			}
			private class UInt16Converter : RuntimeCommandHandler.ArgumentConverter
			{
				public override string TypeName
				{
					get
					{
						return "ushort";
					}
				}
				public override object ConvertString(string s)
				{
					return ushort.Parse(s, 7, CultureInfo.InvariantCulture);
				}
			}
			private class Int32Converter : RuntimeCommandHandler.ArgumentConverter
			{
				public override string TypeName
				{
					get
					{
						return "int";
					}
				}
				public override object ConvertString(string s)
				{
					return int.Parse(s, 7, CultureInfo.InvariantCulture);
				}
			}
			private class UInt32Converter : RuntimeCommandHandler.ArgumentConverter
			{
				public override string TypeName
				{
					get
					{
						return "uint";
					}
				}
				public override object ConvertString(string s)
				{
					return uint.Parse(s, 7, CultureInfo.InvariantCulture);
				}
			}
			private class Int64Converter : RuntimeCommandHandler.ArgumentConverter
			{
				public override string TypeName
				{
					get
					{
						return "long";
					}
				}
				public override object ConvertString(string s)
				{
					return long.Parse(s, 7, CultureInfo.InvariantCulture);
				}
			}
			private class UInt64Converter : RuntimeCommandHandler.ArgumentConverter
			{
				public override string TypeName
				{
					get
					{
						return "ulong";
					}
				}
				public override object ConvertString(string s)
				{
					return ulong.Parse(s, 7, CultureInfo.InvariantCulture);
				}
			}
			private class DecimalConverter : RuntimeCommandHandler.ArgumentConverter
			{
				public override string TypeName
				{
					get
					{
						return "decimal";
					}
				}
				public override object ConvertString(string s)
				{
					return decimal.Parse(s, 167, CultureInfo.InvariantCulture);
				}
			}
			private class SingleConverter : RuntimeCommandHandler.ArgumentConverter
			{
				public override string TypeName
				{
					get
					{
						return "float";
					}
				}
				public override object ConvertString(string s)
				{
					return float.Parse(s, 167, CultureInfo.InvariantCulture);
				}
			}
			private class DoubleConverter : RuntimeCommandHandler.ArgumentConverter
			{
				public override string TypeName
				{
					get
					{
						return "double";
					}
				}
				public override object ConvertString(string s)
				{
					return double.Parse(s, 167, CultureInfo.InvariantCulture);
				}
			}
			private class StringConverter : RuntimeCommandHandler.ArgumentConverter
			{
				public override string TypeName
				{
					get
					{
						return "string";
					}
				}
				public override object ConvertString(string s)
				{
					return s;
				}
			}
		}
		private sealed class ArgumentHandler
		{
			public string Name { get; }
			public bool HasDefaultValue { get; }
			public Type Type { get; }
			public bool IsParams { get; }
			public string TypeName
			{
				get
				{
					return this._converter.TypeName;
				}
			}
			public ArgumentHandler(string name, Type type, bool isParam)
			{
				this.Name = name;
				this.Type = type;
				this.IsParams = isParam;
				this._converter = RuntimeCommandHandler.ArgumentConverter.GetConverter(type);
				this.HasDefaultValue = false;
			}
			public ArgumentHandler(string name, Type type, bool isParam, object defaultValue)
			{
				this.Name = name;
				this.Type = type;
				this.IsParams = isParam;
				this._converter = RuntimeCommandHandler.ArgumentConverter.GetConverter(type);
				this._defaultValue = defaultValue;
				this.HasDefaultValue = true;
			}
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
			private readonly RuntimeCommandHandler.ArgumentConverter _converter;
			private readonly object _defaultValue;
		}
		private sealed class Handler
		{
			public string Name { get; }
			public string Description { get; }
			public string Hint { get; }
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
			private readonly MethodInfo _method;
			private readonly RuntimeCommandHandler.MethodInvoker _invoker;
			private readonly List<RuntimeCommandHandler.ArgumentHandler> _argHandlers = new List<RuntimeCommandHandler.ArgumentHandler>();
		}
		private delegate object MethodInvoker(MethodInfo method, object[] args);
	}
}
