using System;
using System.Collections.Generic;
using UnityEngine;
namespace LBoL.Core
{
	public static class GlobalConfig
	{
		public static string TurboModeTimeScaleString
		{
			get
			{
				return 2f.ToString("G0");
			}
		}
		// Note: this type is marked as 'beforefieldinit'.
		static GlobalConfig()
		{
			Dictionary<char, Color> dictionary = new Dictionary<char, Color>();
			dictionary['b'] = GlobalConfig.DefaultGeneralColor;
			dictionary['e'] = GlobalConfig.EntityColor;
			dictionary['p'] = new Color32(200, 80, 200, byte.MaxValue);
			dictionary['s'] = new Color32(55, 129, 225, byte.MaxValue);
			dictionary['d'] = new Color(1f, 1f, 1f, 0.33f);
			dictionary['f'] = GlobalConfig.UiGreen;
			dictionary['a'] = GlobalConfig.UiBlue;
			dictionary['u'] = GlobalConfig.UiRed;
			dictionary['g'] = GlobalConfig.UpgradedGreen;
			dictionary['k'] = GlobalConfig.DefaultKeywordColor;
			dictionary['n'] = GlobalConfig.NormalColor;
			dictionary['i'] = GlobalConfig.IncreaseColor;
			dictionary['l'] = GlobalConfig.DecreaseColor;
			dictionary['y'] = Color.yellow;
			dictionary['c'] = Color.cyan;
			dictionary['r'] = Color.red;
			GlobalConfig.ColorCodeTable = dictionary;
		}
		public const float LargeTooltipScale = 1.5f;
		public const float HalfDrugRate = 0.5f;
		public const int LowMaxHp = 10;
		public const float LowStageRegenRate = 0.5f;
		public const float PoorBossRate = 0.5f;
		public const float LowUpgradeRate = 0.5f;
		public const float ShopPriceRiseRate = 0.1f;
		public const int UpgradePrice = 25;
		public const int RewardCardCount = 3;
		public const int RewardCardMax = 5;
		public const int DrawCardCount = 5;
		public const int MaxHandInitial = 10;
		public const int MaxHandMax = 12;
		public const int DefaultDollSlot = 4;
		public const int MaxDollSlot = 8;
		public const int DeckCardInstanceIdLimit = 1000;
		public const int MaxDeckCount = 9999;
		public const int MaxZoneCount = 9999;
		public const int MaxMaxHp = 9999;
		public const int MaxBlock = 9999;
		public const int MaxShield = 9999;
		public const int MaxMoney = 99999;
		public const int MaxManaComponentValue = 99;
		public const int MaxCounter = 999;
		public const int MaxUpgradeTimes = 99;
		public const int MaxLoyalty = 9;
		public const int MaxApplyingLevel = 9999;
		public const int MaxLevel = 999;
		public const int MaxApplyingDuration = 9999;
		public const int MaxDuration = 999;
		public const int MaxApplyingCount = 9999;
		public const int MaxCount = 999;
		public const int ShopNormalCardCount = 8;
		public const int ShopToolCardCount = 2;
		public const int ShopExhibitCount = 3;
		public const int FriendWeight = 5;
		public const int ShopUpgradeCardPrice = 50;
		public const int ShopRemoveCardPrice = 50;
		public const int ShopRemoveCardPriceIncrement = 25;
		public static readonly int[] CardPrices = new int[] { 50, 75, 150 };
		public static readonly int[] ExhibitPrices = new int[] { 150, 200, 300 };
		public static readonly int[] ToolPrices = new int[] { 50, 70, 100 };
		public const float PriceRange = 8f;
		public const int OpponentCandidateCount = 3;
		public const float TurboModeTimeScale = 2f;
		public const float DefaultAnimationWaitTime = 0.2f;
		public const float DefaultSeWaitTime = 0.2f;
		public const float BaseTimeStep = 0.016666668f;
		public const int BaseTickPerSecond = 60;
		public const int PieceInGun = 100;
		public const float GunMaxTime = 10f;
		public const float ShootMaxTime = 5f;
		public const float ShootMinTime = 0.1f;
		public const float ShootEndAnimationEarliestTime = 0.5f;
		public const float ShooterLoopMinTime = 0.3f;
		public const float ShooterEndTime = 0.3f;
		public const float CrashTime = 0.3f;
		public const float GunForceEndTime = 0.1f;
		public const float SpellDeclareTime = 2f;
		public const int HealLargeFloor = 12;
		public const int GapDefaultHealRatio = 30;
		public const int RewardCardAbandonMoneyInitial = 5;
		public static readonly int[] EnemyStationMoney = new int[] { 10, 20 };
		public static readonly int[] EnemyStationMoneyHigh = new int[] { 25, 35 };
		public static readonly int[] EliteStationMoney = new int[] { 30, 40 };
		public static readonly int[] BossStationMoney = new int[] { 95, 105 };
		public static readonly Color DefaultGeneralColor = new Color32(82, 112, 239, byte.MaxValue);
		public static readonly Color DefaultKeywordColor = new Color32(239, 200, 81, byte.MaxValue);
		public static readonly Color EntityColor = new Color(0.6f, 1f, 1f, 1f);
		public static readonly Color NormalColor = new Color(0.7f, 1f, 1f, 1f);
		public static readonly Color IncreaseColor = new Color(1f, 0.58f, 0f);
		public static readonly Color DecreaseColor = new Color(1f, 0.6f, 1f);
		public static readonly Color UiRed = new Color32(221, 116, 115, byte.MaxValue);
		public static readonly Color UiGreen = new Color32(139, 200, 138, byte.MaxValue);
		public static readonly Color UiBlue = new Color32(138, 181, 200, byte.MaxValue);
		public static readonly Color UpgradedGreen = new Color32(95, byte.MaxValue, 153, byte.MaxValue);
		public static readonly Color GameResultFail = new Color(0.7f, 0f, 0f, 1f);
		public static readonly Color GameResultNormal = new Color(0.7f, 0.7f, 0.9f, 1f);
		public static readonly Color GameResultTrue = new Color(1f, 1f, 0.8f, 1f);
		public static readonly IReadOnlyDictionary<char, Color> ColorCodeTable;
	}
}
