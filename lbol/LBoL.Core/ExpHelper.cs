using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using UnityEngine;

namespace LBoL.Core
{
	// Token: 0x0200000E RID: 14
	public static class ExpHelper
	{
		// Token: 0x06000089 RID: 137 RVA: 0x000030F8 File Offset: 0x000012F8
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

		// Token: 0x0600008A RID: 138 RVA: 0x000032D0 File Offset: 0x000014D0
		public static void DumpExpData()
		{
			ExpHelper.EnsureInitialized();
			for (int i = 0; i < ExpHelper._expPerLevel.Length; i++)
			{
				Debug.Log(string.Format("Level {0}: {1} (total: {2}", i + 1, ExpHelper._expPerLevel[i], ExpHelper._levelMaxExp[i]));
			}
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x0600008B RID: 139 RVA: 0x00003323 File Offset: 0x00001523
		public static int MaxLevel
		{
			get
			{
				ExpHelper.EnsureInitialized();
				return ExpHelper._levelMaxExp.Length;
			}
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x0600008C RID: 140 RVA: 0x00003331 File Offset: 0x00001531
		public static int MaxExp
		{
			get
			{
				ExpHelper.EnsureInitialized();
				return Enumerable.Last<int>(ExpHelper._levelMaxExp);
			}
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00003344 File Offset: 0x00001544
		public static int GetLevelForTotalExp(int totalExp)
		{
			ExpHelper.EnsureInitialized();
			int num = ExpHelper._levelMaxExp.UpperBound(totalExp);
			int num2 = ExpHelper._levelMaxExp.Length;
			return num;
		}

		// Token: 0x0600008E RID: 142 RVA: 0x0000336D File Offset: 0x0000156D
		public static int GetExpForLevel(int level)
		{
			ExpHelper.EnsureInitialized();
			if (level >= ExpHelper._maxLevel)
			{
				return ExpHelper._expPerLevel[ExpHelper._maxLevel - 1];
			}
			return ExpHelper._expPerLevel[level];
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00003391 File Offset: 0x00001591
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

		// Token: 0x06000090 RID: 144 RVA: 0x000033B4 File Offset: 0x000015B4
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

		// Token: 0x06000091 RID: 145 RVA: 0x000033D8 File Offset: 0x000015D8
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

		// Token: 0x04000069 RID: 105
		private static int _maxLevel;

		// Token: 0x0400006A RID: 106
		private static int[] _expPerLevel;

		// Token: 0x0400006B RID: 107
		private static int[] _levelMaxExp;

		// Token: 0x0400006C RID: 108
		private static Dictionary<string, int> _cardUnlockLevelTable = new Dictionary<string, int>();

		// Token: 0x0400006D RID: 109
		private static Dictionary<string, int> _exhibitUnlockLevelTable = new Dictionary<string, int>();

		// Token: 0x0400006E RID: 110
		private static bool _initialized;
	}
}
