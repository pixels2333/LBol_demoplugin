using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YamlDotNet.Helpers;
using YamlDotNet.RepresentationModel;
namespace LBoL.Core
{
	public static class Achievements
	{
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
		public static IDisplayWord GetAchievementDisplayWord(string key)
		{
			Achievements.AchievementDisplayWord achievementDisplayWord;
			if (!Achievements._table.TryGetValue(key, ref achievementDisplayWord))
			{
				return new Achievements.AchievementDisplayWord(key, "<" + key + ".Name>", "<" + key + ".Description>");
			}
			return new Achievements.AchievementDisplayWord(key, achievementDisplayWord.Name, achievementDisplayWord.Description);
		}
		private static Dictionary<string, Achievements.AchievementDisplayWord> _table = new Dictionary<string, Achievements.AchievementDisplayWord>();
		private sealed class AchievementDisplayWord : IDisplayWord, IEquatable<IDisplayWord>
		{
			public AchievementDisplayWord(string key, string name, string description)
			{
				this.Key = key;
				this.Name = name;
				this.Description = description;
			}
			private string Key { get; }
			public string Name { get; }
			public string Description { get; }
			public bool IsVerbose
			{
				get
				{
					return false;
				}
			}
			public bool Equals(IDisplayWord other)
			{
				Achievements.AchievementDisplayWord achievementDisplayWord = other as Achievements.AchievementDisplayWord;
				return achievementDisplayWord != null && this.Key == achievementDisplayWord.Key;
			}
		}
	}
}
