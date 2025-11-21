using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YamlDotNet.Helpers;
using YamlDotNet.RepresentationModel;

namespace LBoL.Core
{
	// Token: 0x02000006 RID: 6
	public static class Achievements
	{
		// Token: 0x06000006 RID: 6 RVA: 0x000020F8 File Offset: 0x000002F8
		public static async UniTask ReloadLocalizationAsync()
		{
			Achievements._table.Clear();
			try
			{
				foreach (KeyValuePair<YamlNode, YamlNode> keyValuePair in (await Localization.LoadFileAsync("Achievement", true)).Children)
				{
					YamlNode yamlNode;
					YamlNode yamlNode2;
					keyValuePair.Deconstruct(ref yamlNode, ref yamlNode2);
					YamlNode yamlNode3 = yamlNode;
					YamlNode yamlNode4 = yamlNode2;
					YamlScalarNode yamlScalarNode = yamlNode3 as YamlScalarNode;
					if (yamlScalarNode == null)
					{
						Debug.LogError(string.Format("[Localization] Achievement key {0} is not scalar", yamlNode3));
					}
					else
					{
						string value = yamlScalarNode.Value;
						YamlMappingNode yamlMappingNode = yamlNode4 as YamlMappingNode;
						if (yamlMappingNode != null)
						{
							IOrderedDictionary<YamlNode, YamlNode> children = yamlMappingNode.Children;
							YamlNode yamlNode5;
							if (!children.TryGetValue("Name", ref yamlNode5))
							{
								goto IL_012A;
							}
							YamlScalarNode yamlScalarNode2 = yamlNode5 as YamlScalarNode;
							if (yamlScalarNode2 == null)
							{
								goto IL_012A;
							}
							string text = yamlScalarNode2.Value;
							IL_0153:
							YamlNode yamlNode6;
							if (!children.TryGetValue("Description", ref yamlNode6))
							{
								goto IL_017E;
							}
							YamlScalarNode yamlScalarNode3 = yamlNode6 as YamlScalarNode;
							if (yamlScalarNode3 == null)
							{
								goto IL_017E;
							}
							string text2 = yamlScalarNode3.Value;
							IL_01A7:
							if (!Achievements._table.TryAdd(value, new Achievements.AchievementDisplayWord(value, text, text2)))
							{
								Debug.LogError("[Localization] Cannot add duplicated " + value + " to UnitName");
								continue;
							}
							continue;
							IL_017E:
							Debug.LogWarning("[Localization] Achievement of " + value + " has no Description or Description is not scalar");
							text2 = "<" + value + ".Description>";
							goto IL_01A7;
							IL_012A:
							Debug.LogWarning("[Localization] Achievement of " + value + " has no Name or Name is not scalar");
							text = "<" + value + ".Name>";
							goto IL_0153;
						}
						Debug.LogError(string.Format("[Localization] Achievement of {0} is not mapping: {1}", value, yamlNode4));
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("[Localization] failed to load Achievement.yaml: " + ex.Message);
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002134 File Offset: 0x00000334
		public static IDisplayWord GetAchievementDisplayWord(string key)
		{
			Achievements.AchievementDisplayWord achievementDisplayWord;
			if (!Achievements._table.TryGetValue(key, ref achievementDisplayWord))
			{
				return new Achievements.AchievementDisplayWord(key, "<" + key + ".Name>", "<" + key + ".Description>");
			}
			return new Achievements.AchievementDisplayWord(key, achievementDisplayWord.Name, achievementDisplayWord.Description);
		}

		// Token: 0x0400004D RID: 77
		private static Dictionary<string, Achievements.AchievementDisplayWord> _table = new Dictionary<string, Achievements.AchievementDisplayWord>();

		// Token: 0x020001C0 RID: 448
		private sealed class AchievementDisplayWord : IDisplayWord, IEquatable<IDisplayWord>
		{
			// Token: 0x06000FE0 RID: 4064 RVA: 0x0002A62C File Offset: 0x0002882C
			public AchievementDisplayWord(string key, string name, string description)
			{
				this.Key = key;
				this.Name = name;
				this.Description = description;
			}

			// Token: 0x1700055B RID: 1371
			// (get) Token: 0x06000FE1 RID: 4065 RVA: 0x0002A65A File Offset: 0x0002885A
			private string Key { get; }

			// Token: 0x1700055C RID: 1372
			// (get) Token: 0x06000FE2 RID: 4066 RVA: 0x0002A662 File Offset: 0x00028862
			public string Name { get; }

			// Token: 0x1700055D RID: 1373
			// (get) Token: 0x06000FE3 RID: 4067 RVA: 0x0002A66A File Offset: 0x0002886A
			public string Description { get; }

			// Token: 0x1700055E RID: 1374
			// (get) Token: 0x06000FE4 RID: 4068 RVA: 0x0002A672 File Offset: 0x00028872
			public bool IsVerbose
			{
				get
				{
					return false;
				}
			}

			// Token: 0x06000FE5 RID: 4069 RVA: 0x0002A678 File Offset: 0x00028878
			public bool Equals(IDisplayWord other)
			{
				Achievements.AchievementDisplayWord achievementDisplayWord = other as Achievements.AchievementDisplayWord;
				return achievementDisplayWord != null && this.Key == achievementDisplayWord.Key;
			}
		}
	}
}
