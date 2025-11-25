using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using LBoL.Core.Helpers;
using UnityEngine;
using YamlDotNet.Helpers;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
namespace LBoL.Core
{
	public static class Localization
	{
		public static Locale CurrentLocale { get; private set; } = Locale.ZhHans;
		internal static Localization.PluralRuleGetter CardinalRule { get; private set; } = new Localization.PluralRuleGetter(Localization.FallbackRule);
		internal static Localization.PluralRuleGetter OrdinalRule { get; private set; } = new Localization.PluralRuleGetter(Localization.FallbackRule);
		private static Localization.Humanizer CurrentHumanizer { get; set; }
		public static void SetCurrentLocale(Locale locale)
		{
			Localization.CurrentLocale = locale;
			Localization.CardinalRule = Localization.GetCardinalHandler(locale);
			Localization.OrdinalRule = Localization.GetOrdinalHandler(locale);
			Localization.CurrentHumanizer = Localization.GetHumanizer(locale);
		}
		private static YamlMappingNode ParseYaml(string content)
		{
			YamlMappingNode yamlMappingNode;
			using (StringReader stringReader = new StringReader(content))
			{
				YamlStream yamlStream = new YamlStream();
				yamlStream.Load(stringReader);
				yamlMappingNode = ((yamlStream.Documents.Count > 0) ? ((YamlMappingNode)yamlStream.Documents[0].RootNode) : new YamlMappingNode());
			}
			return yamlMappingNode;
		}
		private static async UniTask<string> ReadFileAsync(string name, bool logError = true)
		{
			string path = Path.Combine("Localization", Localization.CurrentLocale.ToTag(), name + ".yaml");
			string text;
			try
			{
				text = await StreamingAssetsHelper.ReadAllTextAsync(path);
			}
			catch (Exception ex)
			{
				if (logError)
				{
					Debug.LogError(string.Format("[Localization] Loading {0} failed: {1}", path, ex));
				}
				text = string.Empty;
			}
			return text;
		}
		internal static async UniTask<YamlMappingNode> LoadFileAsync(string name, bool logError = true)
		{
			string text = await Localization.ReadFileAsync(name, logError);
			string content = text;
			return await UniTask.RunOnThreadPool<YamlMappingNode>(() => Localization.ParseYaml(content), true, default(CancellationToken));
		}
		private static void DefaultSetter(PropertyInfo prop, object obj)
		{
			if (prop.PropertyType == typeof(string))
			{
				prop.SetValue(obj, string.Concat(new string[]
				{
					"<",
					obj.GetType().Name,
					".",
					prop.Name,
					">"
				}));
			}
		}
		private static Dictionary<string, object> CreatePropertyLocalizeTable(string id, YamlMappingNode mapping)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			foreach (KeyValuePair<YamlNode, YamlNode> keyValuePair in mapping)
			{
				YamlNode yamlNode;
				YamlNode yamlNode2;
				keyValuePair.Deconstruct(ref yamlNode, ref yamlNode2);
				YamlNode yamlNode3 = yamlNode;
				YamlNode yamlNode4 = yamlNode2;
				YamlScalarNode yamlScalarNode = yamlNode3 as YamlScalarNode;
				if (yamlScalarNode == null)
				{
					Debug.LogError(string.Format("[Localization] Key {0} is not scalar", yamlNode3));
				}
				else
				{
					YamlScalarNode yamlScalarNode2 = yamlNode4 as YamlScalarNode;
					if (yamlScalarNode2 != null)
					{
						if (!dictionary.TryAdd(yamlScalarNode.Value, yamlScalarNode2.Value))
						{
							Debug.LogError(string.Format("[Localization] Cannot set {0} to {1}", yamlScalarNode, yamlScalarNode2));
						}
					}
					else
					{
						YamlSequenceNode yamlSequenceNode = yamlNode4 as YamlSequenceNode;
						if (yamlSequenceNode != null)
						{
							List<string> list = new List<string>();
							foreach (YamlNode yamlNode5 in yamlSequenceNode)
							{
								YamlScalarNode yamlScalarNode3 = yamlNode5 as YamlScalarNode;
								if (yamlScalarNode3 == null)
								{
									Debug.LogError(string.Format("[Localization] Elem {0} is not scalar", yamlNode5));
									list.Add("<Error>");
								}
								else
								{
									list.Add(yamlScalarNode3.Value);
								}
							}
							if (!dictionary.TryAdd(yamlScalarNode.Value, list.ToArray()))
							{
								Debug.LogError(string.Format("[Localization] Cannot set {0} to {1}", yamlScalarNode, yamlSequenceNode));
							}
						}
						else
						{
							Debug.LogError(string.Format("[Localization] Value {0} is not scalar or sequence", yamlNode4));
						}
					}
				}
			}
			return dictionary;
		}
		internal static async UniTask<Dictionary<string, Dictionary<string, object>>> LoadTypeLocalizationTableAsync(Type baseType, IEnumerable<Type> subTypes)
		{
			YamlMappingNode yamlMappingNode = await Localization.LoadFileAsync(baseType.Name, true);
			YamlMappingNode content = yamlMappingNode;
			return await UniTask.RunOnThreadPool<Dictionary<string, Dictionary<string, object>>>(() => Localization.InternalLoadTypeLocalizationTable(content, baseType, subTypes), true, default(CancellationToken));
		}
		private static Dictionary<string, Dictionary<string, object>> InternalLoadTypeLocalizationTable(YamlMappingNode documentRoot, Type baseType, IEnumerable<Type> subTypes)
		{
			Dictionary<string, Dictionary<string, object>> dictionary = new Dictionary<string, Dictionary<string, object>>();
			try
			{
				IOrderedDictionary<YamlNode, YamlNode> children = documentRoot.Children;
				foreach (Type type in subTypes)
				{
					string name = type.Name;
					try
					{
						YamlNode yamlNode;
						if (children.TryGetValue(name, ref yamlNode))
						{
							YamlMappingNode yamlMappingNode = yamlNode as YamlMappingNode;
							if (yamlMappingNode != null)
							{
								dictionary.Add(name, Localization.CreatePropertyLocalizeTable(type.Name, yamlMappingNode));
								continue;
							}
						}
						dictionary.Add(name, new Dictionary<string, object>());
					}
					catch (Exception ex)
					{
						Debug.LogError(string.Concat(new string[] { "Adding localizer for '", name, "' (", baseType.Name, ") failed: ", ex.Message }));
						dictionary.Add(name, new Dictionary<string, object>());
					}
				}
			}
			catch (Exception ex2)
			{
				Debug.LogError(string.Concat(new string[] { "Localization: Cannot register type '", baseType.Name, "' (", ex2.Message, ")" }));
			}
			return dictionary;
		}
		private static async UniTask LoadTableAsync(string fileName)
		{
			try
			{
				foreach (KeyValuePair<YamlNode, YamlNode> keyValuePair in (await Localization.LoadFileAsync(fileName, true)).Children)
				{
					YamlNode yamlNode;
					YamlNode yamlNode2;
					keyValuePair.Deconstruct(ref yamlNode, ref yamlNode2);
					YamlNode yamlNode3 = yamlNode;
					YamlNode yamlNode4 = yamlNode2;
					YamlScalarNode yamlScalarNode = yamlNode3 as YamlScalarNode;
					if (yamlScalarNode == null)
					{
						Debug.LogError(string.Format("Localization: {0} contains non-scalar key: {1}", fileName, yamlNode3));
					}
					else
					{
						YamlMappingNode yamlMappingNode = yamlNode4 as YamlMappingNode;
						if (yamlMappingNode == null)
						{
							Debug.LogError(string.Format("Localization: {0} contains non-mapping key: {1}: {2}", fileName, yamlNode3, yamlNode4));
						}
						else
						{
							Localization.LoadInnerTable(yamlScalarNode.Value, yamlMappingNode);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Concat(new string[] { "Localization: load '", fileName, "' failed (", ex.Message, ")" }));
			}
		}
		private static void LoadInnerTable(string prefix, YamlMappingNode mapping)
		{
			foreach (KeyValuePair<YamlNode, YamlNode> keyValuePair in mapping.Children)
			{
				YamlNode yamlNode;
				YamlNode yamlNode2;
				keyValuePair.Deconstruct(ref yamlNode, ref yamlNode2);
				YamlNode yamlNode3 = yamlNode;
				YamlNode yamlNode4 = yamlNode2;
				YamlScalarNode yamlScalarNode = yamlNode3 as YamlScalarNode;
				if (yamlScalarNode == null)
				{
					Debug.LogError(string.Format("Non-scalar key: {0}", yamlNode3));
				}
				else if (yamlScalarNode.Value == null)
				{
					Debug.LogError("Empty key");
				}
				else
				{
					YamlScalarNode yamlScalarNode2 = yamlNode4 as YamlScalarNode;
					if (yamlScalarNode2 != null)
					{
						if (!Localization.LocalizationTable.TryAdd(prefix + "." + yamlScalarNode.Value, yamlScalarNode2.Value))
						{
							Debug.LogError(string.Format("Cannot add duplicated {0}.{1} = {2}", prefix, yamlNode3, yamlScalarNode2));
						}
					}
					else
					{
						YamlSequenceNode yamlSequenceNode = yamlNode4 as YamlSequenceNode;
						if (yamlSequenceNode != null)
						{
							string[] array = new string[yamlSequenceNode.Children.Count];
							for (int i = 0; i < array.Length; i++)
							{
								YamlScalarNode yamlScalarNode3 = yamlSequenceNode.Children[i] as YamlScalarNode;
								if (yamlScalarNode3 == null)
								{
									Debug.LogError(string.Format("{0}.{1}[{2}] is not string", prefix, yamlNode3, i));
									array[i] = "<null>";
								}
								else
								{
									array[i] = yamlScalarNode3.Value;
								}
							}
							if (!Localization.LocalizationTable.TryAdd(prefix + "." + yamlScalarNode.Value, array))
							{
								Debug.LogError(string.Format("Cannot add duplicated {0}.{1}({2})", prefix, yamlNode3, array.Length));
							}
						}
						else
						{
							YamlMappingNode yamlMappingNode = yamlNode4 as YamlMappingNode;
							if (yamlMappingNode != null)
							{
								Localization.LoadInnerTable(prefix + "." + yamlScalarNode.Value, yamlMappingNode);
							}
							else
							{
								Debug.LogError(string.Format("Unknown type {0}.{1} = {2}", prefix, yamlNode3, yamlNode4));
							}
						}
					}
				}
			}
		}
		public static string Localize(string key, bool decorate = true)
		{
			object obj;
			if (!Localization.LocalizationTable.TryGetValue(key, ref obj))
			{
				string text;
				if (!Localization.FailureTable.TryGetValue(key, ref text))
				{
					Debug.LogError("[Localization] " + key + " not found");
					text = "<" + key + ">";
					Localization.FailureTable.Add(key, text);
				}
				return text;
			}
			string text2 = obj as string;
			if (text2 == null)
			{
				string text3;
				if (!Localization.FailureTable.TryGetValue(key, ref text3))
				{
					Debug.LogError("[Localization] " + key + " is not string");
					text3 = "<" + key + ">";
					Localization.FailureTable.Add(key, text3);
				}
				return text3;
			}
			string text4;
			try
			{
				text4 = (decorate ? StringDecorator.Decorate(text2) : text2);
			}
			catch (Exception ex)
			{
				Debug.LogError("[Localization] " + key + " decorating failed: " + ex.Message);
				text4 = "<" + key + " (error)>";
			}
			return text4;
		}
		public static string LocalizeFormat(string key, params object[] args)
		{
			object obj;
			if (!Localization.LocalizationTable.TryGetValue(key, ref obj))
			{
				string text;
				if (!Localization.FailureTable.TryGetValue(key, ref text))
				{
					Debug.LogError("[Localization] " + key + " not found");
					text = "<" + key + ">";
					Localization.FailureTable.Add(key, text);
				}
				return text;
			}
			string text2 = obj as string;
			if (text2 == null)
			{
				string text3;
				if (!Localization.FailureTable.TryGetValue(key, ref text3))
				{
					Debug.LogError("[Localization] " + key + " is not string");
					text3 = "<" + key + ">";
					Localization.FailureTable.Add(key, text3);
				}
				return text3;
			}
			try
			{
				text2 = text2.SequenceFormat(args);
			}
			catch (Exception ex)
			{
				Debug.LogError("[Localization] " + key + " formating failed: " + ex.Message);
				return "<" + key + " (error)>";
			}
			try
			{
				text2 = StringDecorator.Decorate(text2);
			}
			catch (Exception ex2)
			{
				Debug.LogError("[Localization] " + key + " decorating failed: " + ex2.Message);
				return "<" + key + " (error)>";
			}
			return text2;
		}
		public static IList<string> LocalizeStrings(string key, bool decorate = true)
		{
			object obj;
			if (!Localization.LocalizationTable.TryGetValue(key, ref obj))
			{
				Debug.LogError("[Localization] " + key + " not found");
				return new FaultTolerantArray<string>(Array.Empty<string>(), "<null>", "[Localization] <" + key + "> not found");
			}
			string[] array = obj as string[];
			if (array == null)
			{
				Debug.LogError("[Localization] " + key + " is not array of strings");
				return new FaultTolerantArray<string>(Array.Empty<string>(), "<null>", "[Localization] <" + key + "> is not array of string");
			}
			if (!decorate)
			{
				return new FaultTolerantArray<string>(Enumerable.Select<string, string>(array, new Func<string, string>(StringDecorator.Decorate)), "<null>", "[Localization] <" + key + ">[{0}] not found");
			}
			return new FaultTolerantArray<string>(array, "<null>", "[Localization] <" + key + ">[{0}] not found");
		}
		public static async UniTask ReloadCommonAsync()
		{
			Localization.LocalizationTable.Clear();
			Localization.FailureTable.Clear();
			await Localization.LoadTableAsync("Common");
		}
		public static async UniTask<Dictionary<string, T>> LoadFileAsync<T>(string name)
		{
			Deserializer deserializer = new Deserializer();
			Deserializer deserializer2 = deserializer;
			string text = await Localization.ReadFileAsync(name, true);
			return deserializer2.Deserialize<Dictionary<string, T>>(text) ?? new Dictionary<string, T>();
		}
		private static Localization.PluralRuleGetter GetCardinalHandler(Locale locale)
		{
			Localization.PluralRuleGetter pluralRuleGetter;
			switch (locale)
			{
			case Locale.En:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.OneOtherCardinalRule);
				break;
			case Locale.ZhHans:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.OneOtherCardinalRule);
				break;
			case Locale.ZhHant:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.OneOtherCardinalRule);
				break;
			case Locale.Ja:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			case Locale.Ru:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.RussianCardinalRule);
				break;
			case Locale.Es:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.OneOtherCardinalRule);
				break;
			case Locale.Pl:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.PolishUkrainianCardinalRule);
				break;
			case Locale.Pt:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FrenchPortugueseCardinalRule);
				break;
			case Locale.Fr:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FrenchPortugueseCardinalRule);
				break;
			case Locale.Tr:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.OneOtherCardinalRule);
				break;
			case Locale.Ko:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			case Locale.Vi:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			case Locale.It:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			case Locale.De:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.OneOtherCardinalRule);
				break;
			case Locale.Uk:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.PolishUkrainianCardinalRule);
				break;
			case Locale.Hu:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			default:
				throw new ArgumentOutOfRangeException("locale", locale, null);
			}
			return pluralRuleGetter;
		}
		private static Localization.PluralRuleGetter GetOrdinalHandler(Locale locale)
		{
			Localization.PluralRuleGetter pluralRuleGetter;
			switch (locale)
			{
			case Locale.En:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.EnglishOrdinalRule);
				break;
			case Locale.ZhHans:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			case Locale.ZhHant:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			case Locale.Ja:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			case Locale.Ru:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			case Locale.Es:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			case Locale.Pl:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			case Locale.Pt:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			case Locale.Fr:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.OneOtherCardinalRule);
				break;
			case Locale.Tr:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			case Locale.Ko:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			case Locale.Vi:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			case Locale.It:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			case Locale.De:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			case Locale.Uk:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			case Locale.Hu:
				pluralRuleGetter = new Localization.PluralRuleGetter(Localization.FallbackRule);
				break;
			default:
				throw new ArgumentOutOfRangeException("locale", locale, null);
			}
			return pluralRuleGetter;
		}
		private static string FallbackRule(int n)
		{
			return null;
		}
		private static string OneOtherCardinalRule(int n)
		{
			if (n != 1)
			{
				return null;
			}
			return "one";
		}
		private static string EnglishOrdinalRule(int n)
		{
			int num = n % 10;
			int num2 = n % 100;
			if (num == 1 && num2 != 11)
			{
				return "one";
			}
			if (num == 2 && num2 != 12)
			{
				return "two";
			}
			if (num == 3 && num2 != 13)
			{
				return "few";
			}
			return null;
		}
		private static string RussianCardinalRule(int n)
		{
			int num = n % 10;
			int num2 = n % 100;
			if (num == 1 && num2 != 11)
			{
				return "one";
			}
			if (num >= 2 && num <= 4 && (num2 < 12 || num2 > 14))
			{
				return "few";
			}
			return "many";
		}
		private static string PolishUkrainianCardinalRule(int n)
		{
			if (n == 1)
			{
				return "one";
			}
			int num = n % 10;
			int num2 = n % 100;
			if (num >= 2 && num <= 4 && (num2 < 12 || num2 > 14))
			{
				return "few";
			}
			return "many";
		}
		private static string FrenchPortugueseCardinalRule(int n)
		{
			if (n <= 1)
			{
				return "one";
			}
			if (n % 1000000 == 0)
			{
				return "many";
			}
			return null;
		}
		public static void TestCardinal(Locale locale, string one, string few, string many, string other)
		{
			Localization.<>c__DisplayClass42_0 CS$<>8__locals1;
			CS$<>8__locals1.other = other;
			CS$<>8__locals1.one = one;
			CS$<>8__locals1.few = few;
			CS$<>8__locals1.many = many;
			StringBuilder stringBuilder = new StringBuilder();
			CS$<>8__locals1.handler = Localization.GetCardinalHandler(locale);
			stringBuilder.AppendLine(Localization.<TestCardinal>g__Get|42_0(0, ref CS$<>8__locals1));
			for (int i = 1; i <= 50; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					stringBuilder.Append(Localization.<TestCardinal>g__Get|42_0(i + 50 * j, ref CS$<>8__locals1).PadRight(16));
				}
				stringBuilder.AppendLine();
			}
			string text = stringBuilder.ToString();
			string text2 = string.Concat(Enumerable.Select<byte, string>(SHA256.Create().ComputeHash(new UTF8Encoding(false, true).GetBytes(text)), (byte n) => n.ToString("x2")));
			Debug.Log(text + "\n\nHash: " + text2);
		}
		private static Localization.Humanizer GetHumanizer(Locale locale)
		{
			switch (locale)
			{
			case Locale.En:
			case Locale.Ru:
			case Locale.Es:
			case Locale.Pl:
			case Locale.Pt:
			case Locale.Tr:
				return new Localization.Humanizer(Localization.UppercaseFirstLetter);
			}
			return new Localization.Humanizer(Localization.Identity);
		}
		private static string Identity(string input)
		{
			return input;
		}
		private static string UppercaseFirstLetter(string input)
		{
			if (input.Length > 0 && char.IsLower(input.get_Chars(0)))
			{
				return char.ToUpper(input.get_Chars(0)).ToString() + input.Substring(1, input.Length - 1);
			}
			return input;
		}
		public static string Humanize(string input)
		{
			Localization.Humanizer currentHumanizer = Localization.CurrentHumanizer;
			return ((currentHumanizer != null) ? currentHumanizer(input) : null) ?? input;
		}
		[CompilerGenerated]
		internal static string <TestCardinal>g__Get|42_0(int n, ref Localization.<>c__DisplayClass42_0 A_1)
		{
			string text = A_1.handler(n);
			string text2 = n.ToString().PadLeft(2);
			string text3;
			if (text != null)
			{
				if (!(text == "one"))
				{
					if (!(text == "few"))
					{
						if (!(text == "many"))
						{
							throw new ArgumentOutOfRangeException();
						}
						text3 = A_1.many ?? A_1.other;
					}
					else
					{
						text3 = A_1.few ?? A_1.other;
					}
				}
				else
				{
					text3 = A_1.one ?? A_1.other;
				}
			}
			else
			{
				text3 = A_1.other;
			}
			return text2 + " " + (text3 ?? ("<" + text + ">"));
		}
		private static readonly Dictionary<string, object> LocalizationTable = new Dictionary<string, object>();
		private static readonly Dictionary<string, string> FailureTable = new Dictionary<string, string>();
		internal delegate string PluralRuleGetter(int n);
		private delegate string Humanizer(string input);
	}
}
