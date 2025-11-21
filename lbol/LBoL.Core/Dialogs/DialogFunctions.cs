using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core.Stations;
using Yarn;

namespace LBoL.Core.Dialogs
{
	// Token: 0x02000120 RID: 288
	public class DialogFunctions
	{
		// Token: 0x06000A32 RID: 2610 RVA: 0x0001CCF0 File Offset: 0x0001AEF0
		[DialogFunction("escape")]
		[UsedImplicitly]
		public static string Escape(string s)
		{
			if (string.IsNullOrWhiteSpace(s))
			{
				return "\"" + s + "\"";
			}
			if (Enumerable.All<char>(s, (char c) => !char.IsControl(c) && !char.IsWhiteSpace(c) && c != '\\' && c != '"'))
			{
				return s;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append('"');
			for (int i = 0; i < s.Length; i++)
			{
				char c2 = s.get_Chars(i);
				if (c2 == '\\' || c2 == '"')
				{
					stringBuilder.Append('\\');
				}
				stringBuilder.Append(c2);
			}
			stringBuilder.Append('"');
			return stringBuilder.ToString();
		}

		// Token: 0x06000A33 RID: 2611 RVA: 0x0001CD93 File Offset: 0x0001AF93
		[DialogFunction("getPlayerId")]
		[UsedImplicitly]
		public string GetPlayerId()
		{
			return this.GetGameRun().Player.Id;
		}

		// Token: 0x06000A34 RID: 2612 RVA: 0x0001CDA5 File Offset: 0x0001AFA5
		[DialogFunction("getPlayerName")]
		[UsedImplicitly]
		public string GetPlayerName()
		{
			return "@PlayerName";
		}

		// Token: 0x06000A35 RID: 2613 RVA: 0x0001CDAC File Offset: 0x0001AFAC
		[DialogFunction("getPlayerShortName")]
		[UsedImplicitly]
		public string GetPlayerShortName()
		{
			return "@PlayerShortName";
		}

		// Token: 0x06000A36 RID: 2614 RVA: 0x0001CDB3 File Offset: 0x0001AFB3
		[DialogFunction("getEnemyName")]
		[UsedImplicitly]
		public string GetEnemyName(string enemyId)
		{
			return Library.CreateEnemyUnit(enemyId).GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Default);
		}

		// Token: 0x06000A37 RID: 2615 RVA: 0x0001CDC8 File Offset: 0x0001AFC8
		[DialogFunction("getExhibitName")]
		[UsedImplicitly]
		public string GetExhibitName(string id)
		{
			return "@Exhibit-" + id;
		}

		// Token: 0x06000A38 RID: 2616 RVA: 0x0001CDD5 File Offset: 0x0001AFD5
		[DialogFunction("getCardName")]
		[UsedImplicitly]
		public string GetCardName(string id)
		{
			return "@Card-" + id;
		}

		// Token: 0x06000A39 RID: 2617 RVA: 0x0001CDE4 File Offset: 0x0001AFE4
		[DialogFunction("hasClearBonus")]
		[UsedImplicitly]
		public bool HasClearBonus()
		{
			if (!this.GetGameRun().HasClearBonus)
			{
				Station currentStation = this.GetGameRun().CurrentStation;
				return currentStation != null && currentStation.Type == StationType.BattleAdvTest;
			}
			return true;
		}

		// Token: 0x06000A3A RID: 2618 RVA: 0x0001CE1B File Offset: 0x0001B01B
		[DialogFunction("isEasy")]
		[UsedImplicitly]
		public bool IsEasy()
		{
			return this.GetGameRun().Difficulty == GameDifficulty.Easy;
		}

		// Token: 0x06000A3B RID: 2619 RVA: 0x0001CE2B File Offset: 0x0001B02B
		[DialogFunction("isNormal")]
		[UsedImplicitly]
		public bool IsNormal()
		{
			return this.GetGameRun().Difficulty == GameDifficulty.Normal;
		}

		// Token: 0x06000A3C RID: 2620 RVA: 0x0001CE3B File Offset: 0x0001B03B
		[DialogFunction("isHard")]
		[UsedImplicitly]
		public bool IsHard()
		{
			return this.GetGameRun().Difficulty == GameDifficulty.Hard;
		}

		// Token: 0x06000A3D RID: 2621 RVA: 0x0001CE4B File Offset: 0x0001B04B
		[DialogFunction("isLunatic")]
		[UsedImplicitly]
		public bool IsLunatic()
		{
			return this.GetGameRun().Difficulty == GameDifficulty.Lunatic;
		}

		// Token: 0x06000A3E RID: 2622 RVA: 0x0001CE5B File Offset: 0x0001B05B
		[DialogFunction("isAutoSeed")]
		[UsedImplicitly]
		public bool IsAutoSeed()
		{
			return this.GetGameRun().IsAutoSeed;
		}

		// Token: 0x06000A3F RID: 2623 RVA: 0x0001CE68 File Offset: 0x0001B068
		[DialogFunction("hasExhibit")]
		[UsedImplicitly]
		public bool HasExhibit(string id)
		{
			return this.GetGameRun().Player.HasExhibit(id);
		}

		// Token: 0x06000A40 RID: 2624 RVA: 0x0001CE7B File Offset: 0x0001B07B
		[DialogFunction("hasTrueEndProvider")]
		[UsedImplicitly]
		public bool HasTrueEndProvider()
		{
			return this.GetGameRun().TrueEndingProviders.NotEmpty<GameEntity>();
		}

		// Token: 0x06000A41 RID: 2625 RVA: 0x0001CE8D File Offset: 0x0001B08D
		[DialogFunction("trueEndProviderName")]
		[UsedImplicitly]
		public string TrueEndProviderName()
		{
			return Enumerable.First<GameEntity>(this.GetGameRun().TrueEndingProviders).Name;
		}

		// Token: 0x06000A42 RID: 2626 RVA: 0x0001CEA4 File Offset: 0x0001B0A4
		[DialogFunction("trueEndBlockerName")]
		[UsedImplicitly]
		public string TrueEndBlockerName()
		{
			return Enumerable.First<GameEntity>(this.GetGameRun().TrueEndingBlockers).Name;
		}

		// Token: 0x06000A43 RID: 2627 RVA: 0x0001CEBC File Offset: 0x0001B0BC
		[DialogFunction("isTrueEndBlocked")]
		[UsedImplicitly]
		public bool IsTrueEndBlocked()
		{
			GameRunController gameRun = this.GetGameRun();
			return gameRun.TrueEndingProviders.NotEmpty<GameEntity>() && gameRun.TrueEndingBlockers.NotEmpty<GameEntity>();
		}

		// Token: 0x06000A44 RID: 2628 RVA: 0x0001CEEA File Offset: 0x0001B0EA
		[DialogFunction("getMoney")]
		[UsedImplicitly]
		public int GetMoney()
		{
			return this.GetGameRun().Money;
		}

		// Token: 0x06000A45 RID: 2629 RVA: 0x0001CEF7 File Offset: 0x0001B0F7
		[DialogFunction("hasMoney")]
		[UsedImplicitly]
		public bool HasMoney(int money)
		{
			return this.GetGameRun().Money >= money;
		}

		// Token: 0x06000A46 RID: 2630 RVA: 0x0001CF0A File Offset: 0x0001B10A
		[DialogFunction("getPower")]
		[UsedImplicitly]
		public int GetPower()
		{
			return this.GetGameRun().Player.Power;
		}

		// Token: 0x06000A47 RID: 2631 RVA: 0x0001CF1C File Offset: 0x0001B11C
		[DialogFunction("hasPower")]
		[UsedImplicitly]
		public bool HasPower(int power)
		{
			return this.GetGameRun().Player.Power >= power;
		}

		// Token: 0x06000A48 RID: 2632 RVA: 0x0001CF34 File Offset: 0x0001B134
		[DialogFunction("adventureRandom")]
		[UsedImplicitly]
		public int AdventureRand(int a, int b)
		{
			return this.GetGameRun().AdventureRng.NextInt(a, b);
		}

		// Token: 0x06000A49 RID: 2633 RVA: 0x0001CF48 File Offset: 0x0001B148
		[DialogFunction("getStageLevel")]
		[UsedImplicitly]
		public int GetStageLevel()
		{
			return this.GetGameRun().CurrentStage.Level;
		}

		// Token: 0x06000A4A RID: 2634 RVA: 0x0001CF5A File Offset: 0x0001B15A
		[DialogFunction("getStationLevel")]
		[UsedImplicitly]
		public int GetStationLevel()
		{
			return this.GetGameRun().CurrentStation.Level;
		}

		// Token: 0x06000A4B RID: 2635 RVA: 0x0001CF6C File Offset: 0x0001B16C
		[DialogFunction("hasGameRunFlag")]
		[UsedImplicitly]
		public bool HasGameRunFlag(string flag)
		{
			return this.GetGameRun().ExtraFlags.Contains(flag);
		}

		// Token: 0x06000A4C RID: 2636 RVA: 0x0001CF7F File Offset: 0x0001B17F
		private GameRunController GetGameRun()
		{
			if (this._gameRun == null)
			{
				throw new InvalidOperationException("Cannot invoke game-run functions without game-run");
			}
			return this._gameRun;
		}

		// Token: 0x06000A4D RID: 2637 RVA: 0x0001CF9A File Offset: 0x0001B19A
		public DialogFunctions(GameRunController gameRun)
		{
			this._gameRun = gameRun;
		}

		// Token: 0x06000A4E RID: 2638 RVA: 0x0001CFAC File Offset: 0x0001B1AC
		public Library ToLibrary()
		{
			Library library = new Library();
			foreach (MethodInfo methodInfo in base.GetType().GetMethods(28))
			{
				DialogFunctionAttribute customAttribute = CustomAttributeExtensions.GetCustomAttribute<DialogFunctionAttribute>(methodInfo);
				if (customAttribute != null)
				{
					library.RegisterFunction(customAttribute.Name, methodInfo.IsStatic ? methodInfo.CreateDelegate(DialogFunctions.GetEquivalentDelegateTypeFor(methodInfo)) : methodInfo.CreateDelegate(DialogFunctions.GetEquivalentDelegateTypeFor(methodInfo), this));
				}
			}
			return library;
		}

		// Token: 0x06000A4F RID: 2639 RVA: 0x0001D01C File Offset: 0x0001B21C
		private static Type GetEquivalentDelegateTypeFor(MethodInfo methodInfo)
		{
			ParameterInfo[] parameters = methodInfo.GetParameters();
			Type[] array = new Type[parameters.Length + 1];
			for (int i = 0; i < parameters.Length; i++)
			{
				array[i] = parameters[i].ParameterType;
			}
			Type[] array2 = array;
			array2[array2.Length - 1] = methodInfo.ReturnType;
			return Expression.GetDelegateType(array);
		}

		// Token: 0x04000511 RID: 1297
		private readonly GameRunController _gameRun;
	}
}
