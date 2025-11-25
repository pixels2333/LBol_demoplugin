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
	public class DialogFunctions
	{
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
		[DialogFunction("getPlayerId")]
		[UsedImplicitly]
		public string GetPlayerId()
		{
			return this.GetGameRun().Player.Id;
		}
		[DialogFunction("getPlayerName")]
		[UsedImplicitly]
		public string GetPlayerName()
		{
			return "@PlayerName";
		}
		[DialogFunction("getPlayerShortName")]
		[UsedImplicitly]
		public string GetPlayerShortName()
		{
			return "@PlayerShortName";
		}
		[DialogFunction("getEnemyName")]
		[UsedImplicitly]
		public string GetEnemyName(string enemyId)
		{
			return Library.CreateEnemyUnit(enemyId).GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Default);
		}
		[DialogFunction("getExhibitName")]
		[UsedImplicitly]
		public string GetExhibitName(string id)
		{
			return "@Exhibit-" + id;
		}
		[DialogFunction("getCardName")]
		[UsedImplicitly]
		public string GetCardName(string id)
		{
			return "@Card-" + id;
		}
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
		[DialogFunction("isEasy")]
		[UsedImplicitly]
		public bool IsEasy()
		{
			return this.GetGameRun().Difficulty == GameDifficulty.Easy;
		}
		[DialogFunction("isNormal")]
		[UsedImplicitly]
		public bool IsNormal()
		{
			return this.GetGameRun().Difficulty == GameDifficulty.Normal;
		}
		[DialogFunction("isHard")]
		[UsedImplicitly]
		public bool IsHard()
		{
			return this.GetGameRun().Difficulty == GameDifficulty.Hard;
		}
		[DialogFunction("isLunatic")]
		[UsedImplicitly]
		public bool IsLunatic()
		{
			return this.GetGameRun().Difficulty == GameDifficulty.Lunatic;
		}
		[DialogFunction("isAutoSeed")]
		[UsedImplicitly]
		public bool IsAutoSeed()
		{
			return this.GetGameRun().IsAutoSeed;
		}
		[DialogFunction("hasExhibit")]
		[UsedImplicitly]
		public bool HasExhibit(string id)
		{
			return this.GetGameRun().Player.HasExhibit(id);
		}
		[DialogFunction("hasTrueEndProvider")]
		[UsedImplicitly]
		public bool HasTrueEndProvider()
		{
			return this.GetGameRun().TrueEndingProviders.NotEmpty<GameEntity>();
		}
		[DialogFunction("trueEndProviderName")]
		[UsedImplicitly]
		public string TrueEndProviderName()
		{
			return Enumerable.First<GameEntity>(this.GetGameRun().TrueEndingProviders).Name;
		}
		[DialogFunction("trueEndBlockerName")]
		[UsedImplicitly]
		public string TrueEndBlockerName()
		{
			return Enumerable.First<GameEntity>(this.GetGameRun().TrueEndingBlockers).Name;
		}
		[DialogFunction("isTrueEndBlocked")]
		[UsedImplicitly]
		public bool IsTrueEndBlocked()
		{
			GameRunController gameRun = this.GetGameRun();
			return gameRun.TrueEndingProviders.NotEmpty<GameEntity>() && gameRun.TrueEndingBlockers.NotEmpty<GameEntity>();
		}
		[DialogFunction("getMoney")]
		[UsedImplicitly]
		public int GetMoney()
		{
			return this.GetGameRun().Money;
		}
		[DialogFunction("hasMoney")]
		[UsedImplicitly]
		public bool HasMoney(int money)
		{
			return this.GetGameRun().Money >= money;
		}
		[DialogFunction("getPower")]
		[UsedImplicitly]
		public int GetPower()
		{
			return this.GetGameRun().Player.Power;
		}
		[DialogFunction("hasPower")]
		[UsedImplicitly]
		public bool HasPower(int power)
		{
			return this.GetGameRun().Player.Power >= power;
		}
		[DialogFunction("adventureRandom")]
		[UsedImplicitly]
		public int AdventureRand(int a, int b)
		{
			return this.GetGameRun().AdventureRng.NextInt(a, b);
		}
		[DialogFunction("getStageLevel")]
		[UsedImplicitly]
		public int GetStageLevel()
		{
			return this.GetGameRun().CurrentStage.Level;
		}
		[DialogFunction("getStationLevel")]
		[UsedImplicitly]
		public int GetStationLevel()
		{
			return this.GetGameRun().CurrentStation.Level;
		}
		[DialogFunction("hasGameRunFlag")]
		[UsedImplicitly]
		public bool HasGameRunFlag(string flag)
		{
			return this.GetGameRun().ExtraFlags.Contains(flag);
		}
		private GameRunController GetGameRun()
		{
			if (this._gameRun == null)
			{
				throw new InvalidOperationException("Cannot invoke game-run functions without game-run");
			}
			return this._gameRun;
		}
		public DialogFunctions(GameRunController gameRun)
		{
			this._gameRun = gameRun;
		}
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
		private readonly GameRunController _gameRun;
	}
}
