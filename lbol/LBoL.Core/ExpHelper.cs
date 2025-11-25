using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using UnityEngine;
namespace LBoL.Core
{
	public static class ExpHelper
	{
		private static void EnsureInitialized()
		{
			if (ExpHelper._initialized)
			{
				return;
			}
			SortedDictionary<int, ExpConfig> sortedDictionary = new SortedDictionary<int, ExpConfig>();
			foreach (ExpConfig expConfig in ExpConfig.AllConfig())
			{
				CollectionExtensions.TryAdd<int, ExpConfig>(sortedDictionary, expConfig.Level, expConfig);
			}
			int count = sortedDictionary.Count;
			ExpHelper._maxLevel = count;
			ExpHelper._expPerLevel = new int[count];
			ExpHelper._levelMaxExp = new int[count];
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				int num2 = i + 1;
				ExpConfig expConfig2;
				if (!sortedDictionary.TryGetValue(num2, ref expConfig2))
				{
					throw new InvalidDataException(string.Format("Cannot find exp-level {0} data`", num2));
				}
				num += expConfig2.Exp;
				ExpHelper._expPerLevel[i] = expConfig2.Exp;
				ExpHelper._levelMaxExp[i] = num;
				foreach (string text in expConfig2.UnlockCards)
				{
					int num3;
					if (ExpHelper._cardUnlockLevelTable.TryGetValue(text, ref num3))
					{
						Debug.LogError(string.Format("[ExpConfig] Card {0} is already in level {1}", text, num3));
					}
					else
					{
						ExpHelper._cardUnlockLevelTable.Add(text, num2);
					}
				}
				foreach (string text2 in expConfig2.UnlockExhibits)
				{
					int num4;
					if (ExpHelper._exhibitUnlockLevelTable.TryGetValue(text2, ref num4))
					{
						Debug.LogError(string.Format("[ExpConfig] Exhibit {0} is already in level {1}", text2, num4));
					}
					else
					{
						ExpHelper._exhibitUnlockLevelTable.Add(text2, num2);
					}
				}
			}
			ExpHelper._initialized = true;
		}
		public static void DumpExpData()
		{
			ExpHelper.EnsureInitialized();
			for (int i = 0; i < ExpHelper._expPerLevel.Length; i++)
			{
				Debug.Log(string.Format("Level {0}: {1} (total: {2}", i + 1, ExpHelper._expPerLevel[i], ExpHelper._levelMaxExp[i]));
			}
		}
		public static int MaxLevel
		{
			get
			{
				ExpHelper.EnsureInitialized();
				return ExpHelper._levelMaxExp.Length;
			}
		}
		public static int MaxExp
		{
			get
			{
				ExpHelper.EnsureInitialized();
				return Enumerable.Last<int>(ExpHelper._levelMaxExp);
			}
		}
		public static int GetLevelForTotalExp(int totalExp)
		{
			ExpHelper.EnsureInitialized();
			int num = ExpHelper._levelMaxExp.UpperBound(totalExp);
			int num2 = ExpHelper._levelMaxExp.Length;
			return num;
		}
		public static int GetExpForLevel(int level)
		{
			ExpHelper.EnsureInitialized();
			if (level >= ExpHelper._maxLevel)
			{
				return ExpHelper._expPerLevel[ExpHelper._maxLevel - 1];
			}
			return ExpHelper._expPerLevel[level];
		}
		public static int GetMaxExpForLevel(int level)
		{
			ExpHelper.EnsureInitialized();
			if (level < 0)
			{
				return 0;
			}
			if (level >= ExpHelper._maxLevel)
			{
				return 9999999;
			}
			return ExpHelper._levelMaxExp[level];
		}
		public static int GetCardUnlockLevel(string id)
		{
			ExpHelper.EnsureInitialized();
			int num;
			if (!ExpHelper._cardUnlockLevelTable.TryGetValue(id, ref num))
			{
				return -1;
			}
			return num;
		}
		public static int GetExhibitUnlockLevel(string id)
		{
			ExpHelper.EnsureInitialized();
			int num;
			if (!ExpHelper._exhibitUnlockLevelTable.TryGetValue(id, ref num))
			{
				return -1;
			}
			return num;
		}
		private static int _maxLevel;
		private static int[] _expPerLevel;
		private static int[] _levelMaxExp;
		private static Dictionary<string, int> _cardUnlockLevelTable = new Dictionary<string, int>();
		private static Dictionary<string, int> _exhibitUnlockLevelTable = new Dictionary<string, int>();
		private static bool _initialized;
	}
}
