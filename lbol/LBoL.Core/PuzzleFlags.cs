using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LBoL.Core.Helpers;
using UnityEngine;
using YamlDotNet.RepresentationModel;
namespace LBoL.Core
{
	public static class PuzzleFlags
	{
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
		public static int GetPuzzleLevel(PuzzleFlag puzzle)
		{
			return Enumerable.Count<PuzzleFlag>(PuzzleFlags.EnumerateComponents(puzzle));
		}
		public static IEnumerable<PuzzleFlag> EnumerateComponents(PuzzleFlag puzzleFlag)
		{
			return Enumerable.Where<PuzzleFlag>(PuzzleFlags.AllPuzzleFlags, (PuzzleFlag c) => puzzleFlag.HasFlag(c));
		}
		public static bool IsAll(PuzzleFlag puzzleFlag)
		{
			return Enumerable.All<PuzzleFlag>(PuzzleFlags.AllPuzzleFlags, (PuzzleFlag f) => puzzleFlag.HasFlag(f));
		}
		public static PuzzleFlag FromComponents(IEnumerable<PuzzleFlag> puzzleFlags)
		{
			return Enumerable.Aggregate<PuzzleFlag, PuzzleFlag>(puzzleFlags, PuzzleFlag.None, (PuzzleFlag current, PuzzleFlag flag) => current | flag);
		}
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
		public static readonly IReadOnlyList<PuzzleFlag> AllPuzzleFlags = Enumerable.ToList<PuzzleFlag>(Enumerable.Where<PuzzleFlag>(EnumHelper<PuzzleFlag>.GetValues(), (PuzzleFlag k) => k > PuzzleFlag.None && k <= PuzzleFlag.NightMana)).AsReadOnly();
		private static readonly Dictionary<PuzzleFlag, PuzzleFlagDisplayWord> PuzzleFlagTable = new Dictionary<PuzzleFlag, PuzzleFlagDisplayWord>();
	}
}
