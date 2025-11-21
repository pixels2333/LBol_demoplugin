using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LBoL.Core.Helpers;
using UnityEngine;
using YamlDotNet.RepresentationModel;

namespace LBoL.Core
{
	// Token: 0x02000065 RID: 101
	public static class PuzzleFlags
	{
		// Token: 0x0600044F RID: 1103 RVA: 0x0000EAFC File Offset: 0x0000CCFC
		public static async UniTask ReloadAsync()
		{
			PuzzleFlags.PuzzleFlagTable.Clear();
			YamlMappingNode yamlMappingNode;
			try
			{
				yamlMappingNode = await Localization.LoadFileAsync("Puzzle", true);
			}
			catch (Exception ex)
			{
				Debug.LogError("[Localization] Faild to localize puzzle flag: " + ex.Message);
				yamlMappingNode = new YamlMappingNode();
			}
			foreach (PuzzleFlag puzzleFlag in PuzzleFlags.AllPuzzleFlags)
			{
				string text = EnumHelper<PuzzleFlag>.GetName(puzzleFlag);
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
						Debug.LogError("[Localization] PuzzleFlag localization for <" + text + "> is not table");
					}
				}
				else
				{
					Debug.LogError("[Localization] PuzzleFlag localization not found for <" + text + ">");
				}
				PuzzleFlags.PuzzleFlagTable.Add(puzzleFlag, new PuzzleFlagDisplayWord(puzzleFlag, text, StringDecorator.Decorate(text2)));
			}
		}

		// Token: 0x06000450 RID: 1104 RVA: 0x0000EB37 File Offset: 0x0000CD37
		public static int GetPuzzleLevel(PuzzleFlag puzzle)
		{
			return Enumerable.Count<PuzzleFlag>(PuzzleFlags.EnumerateComponents(puzzle));
		}

		// Token: 0x06000451 RID: 1105 RVA: 0x0000EB44 File Offset: 0x0000CD44
		public static IEnumerable<PuzzleFlag> EnumerateComponents(PuzzleFlag puzzleFlag)
		{
			return Enumerable.Where<PuzzleFlag>(PuzzleFlags.AllPuzzleFlags, (PuzzleFlag c) => puzzleFlag.HasFlag(c));
		}

		// Token: 0x06000452 RID: 1106 RVA: 0x0000EB74 File Offset: 0x0000CD74
		public static bool IsAll(PuzzleFlag puzzleFlag)
		{
			return Enumerable.All<PuzzleFlag>(PuzzleFlags.AllPuzzleFlags, (PuzzleFlag f) => puzzleFlag.HasFlag(f));
		}

		// Token: 0x06000453 RID: 1107 RVA: 0x0000EBA4 File Offset: 0x0000CDA4
		public static PuzzleFlag FromComponents(IEnumerable<PuzzleFlag> puzzleFlags)
		{
			return Enumerable.Aggregate<PuzzleFlag, PuzzleFlag>(puzzleFlags, PuzzleFlag.None, (PuzzleFlag current, PuzzleFlag flag) => current | flag);
		}

		// Token: 0x06000454 RID: 1108 RVA: 0x0000EBCC File Offset: 0x0000CDCC
		public static PuzzleFlagDisplayWord GetDisplayWord(PuzzleFlag puzzleFlag)
		{
			PuzzleFlagDisplayWord puzzleFlagDisplayWord;
			if (PuzzleFlags.PuzzleFlagTable.TryGetValue(puzzleFlag, ref puzzleFlagDisplayWord))
			{
				return puzzleFlagDisplayWord;
			}
			Debug.LogError(string.Format("Cannot get puzzle flag display word for '{0}'", puzzleFlag));
			return null;
		}

		// Token: 0x04000257 RID: 599
		public static readonly IReadOnlyList<PuzzleFlag> AllPuzzleFlags = Enumerable.ToList<PuzzleFlag>(Enumerable.Where<PuzzleFlag>(EnumHelper<PuzzleFlag>.GetValues(), (PuzzleFlag k) => k > PuzzleFlag.None && k <= PuzzleFlag.NightMana)).AsReadOnly();

		// Token: 0x04000258 RID: 600
		private static readonly Dictionary<PuzzleFlag, PuzzleFlagDisplayWord> PuzzleFlagTable = new Dictionary<PuzzleFlag, PuzzleFlagDisplayWord>();
	}
}
