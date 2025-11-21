using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using LBoL.Base;
using LBoL.Core.Helpers;
using UnityEngine;
using YamlDotNet.RepresentationModel;

namespace LBoL.Core
{
	// Token: 0x02000057 RID: 87
	public static class Keywords
	{
		// Token: 0x060003A4 RID: 932 RVA: 0x0000BC44 File Offset: 0x00009E44
		public static async UniTask ReloadAsync()
		{
			Keywords.KeywordTable.Clear();
			YamlMappingNode yamlMappingNode;
			try
			{
				yamlMappingNode = await Localization.LoadFileAsync("Keyword", true);
			}
			catch (Exception ex)
			{
				Debug.LogError("Faild to localize keywords: " + ex.Message);
				yamlMappingNode = new YamlMappingNode();
			}
			foreach (Keyword keyword in Keywords.AllKeywords)
			{
				string text = EnumHelper<Keyword>.GetName(keyword);
				string text2 = "<" + text + ".Description>";
				YamlNode yamlNode;
				if (yamlMappingNode.Children.TryGetValue(text, ref yamlNode))
				{
					YamlMappingNode yamlMappingNode2 = yamlNode as YamlMappingNode;
					if (yamlMappingNode2 != null)
					{
						YamlNode yamlNode2;
						if (yamlMappingNode2.Children.TryGetValue("Name", ref yamlNode2))
						{
							YamlScalarNode yamlScalarNode = yamlNode2 as YamlScalarNode;
							if (yamlScalarNode != null)
							{
								text = yamlScalarNode.Value;
							}
						}
						YamlNode yamlNode3;
						if (yamlMappingNode2.Children.TryGetValue("Description", ref yamlNode3))
						{
							YamlScalarNode yamlScalarNode2 = yamlNode3 as YamlScalarNode;
							if (yamlScalarNode2 != null)
							{
								text2 = yamlScalarNode2.Value;
							}
						}
					}
					else
					{
						Debug.LogError("[Localization] Keyword for <" + text + "> is not table");
					}
				}
				else
				{
					Debug.LogError("[Localization] Keyword not found for <" + text + ">");
				}
				KeywordAttribute customAttribute = CustomAttributeExtensions.GetCustomAttribute<KeywordAttribute>(Enumerable.First<MemberInfo>(typeof(Keyword).GetMember(keyword.ToString())));
				bool flag = customAttribute == null || customAttribute.AutoAppend;
				bool flag2 = customAttribute != null && customAttribute.IsVerbose;
				bool flag3 = customAttribute != null && customAttribute.Hidden;
				Keywords.KeywordTable.Add(keyword, new KeywordDisplayWord(keyword, text, StringDecorator.Decorate(text2), flag, flag2, flag3));
			}
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x0000BC80 File Offset: 0x00009E80
		public static IEnumerable<Keyword> EnumerateComponents(Keyword keyword)
		{
			return Enumerable.Where<Keyword>(Keywords.AllKeywords, (Keyword c) => keyword.HasFlag(c));
		}

		// Token: 0x060003A6 RID: 934 RVA: 0x0000BCB0 File Offset: 0x00009EB0
		public static KeywordDisplayWord GetDisplayWord(Keyword keyword)
		{
			KeywordDisplayWord keywordDisplayWord;
			if (Keywords.KeywordTable.TryGetValue(keyword, ref keywordDisplayWord))
			{
				return keywordDisplayWord;
			}
			Debug.LogError(string.Format("Cannot get keyword display word for '{0}'", keyword));
			return null;
		}

		// Token: 0x04000215 RID: 533
		private static readonly List<Keyword> AllKeywords = Enumerable.ToList<Keyword>(Enumerable.Where<Keyword>(EnumHelper<Keyword>.GetValues(), (Keyword k) => k > Keyword.None));

		// Token: 0x04000216 RID: 534
		private static readonly Dictionary<Keyword, KeywordDisplayWord> KeywordTable = new Dictionary<Keyword, KeywordDisplayWord>();
	}
}
