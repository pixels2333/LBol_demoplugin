using System;
using System.Collections.Generic;
using UnityEngine;

namespace LBoL.Core
{
	// Token: 0x02000047 RID: 71
	public static class GlobalConfig
	{
		// Token: 0x1700012E RID: 302
		// (get) Token: 0x06000346 RID: 838 RVA: 0x0000B35C File Offset: 0x0000955C
		public static string TurboModeTimeScaleString
		{
			get
			{
				return 2f.ToString("G0");
			}
		}

		// Token: 0x06000347 RID: 839 RVA: 0x0000B37C File Offset: 0x0000957C
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

		// Token: 0x040001AD RID: 429
		public const float LargeTooltipScale = 1.5f;

		// Token: 0x040001AE RID: 430
		public const float HalfDrugRate = 0.5f;

		// Token: 0x040001AF RID: 431
		public const int LowMaxHp = 10;

		// Token: 0x040001B0 RID: 432
		public const float LowStageRegenRate = 0.5f;

		// Token: 0x040001B1 RID: 433
		public const float PoorBossRate = 0.5f;

		// Token: 0x040001B2 RID: 434
		public const float LowUpgradeRate = 0.5f;

		// Token: 0x040001B3 RID: 435
		public const float ShopPriceRiseRate = 0.1f;

		// Token: 0x040001B4 RID: 436
		public const int UpgradePrice = 25;

		// Token: 0x040001B5 RID: 437
		public const int RewardCardCount = 3;

		// Token: 0x040001B6 RID: 438
		public const int RewardCardMax = 5;

		// Token: 0x040001B7 RID: 439
		public const int DrawCardCount = 5;

		// Token: 0x040001B8 RID: 440
		public const int MaxHandInitial = 10;

		// Token: 0x040001B9 RID: 441
		public const int MaxHandMax = 12;

		// Token: 0x040001BA RID: 442
		public const int DefaultDollSlot = 4;

		// Token: 0x040001BB RID: 443
		public const int MaxDollSlot = 8;

		// Token: 0x040001BC RID: 444
		public const int DeckCardInstanceIdLimit = 1000;

		// Token: 0x040001BD RID: 445
		public const int MaxDeckCount = 9999;

		// Token: 0x040001BE RID: 446
		public const int MaxZoneCount = 9999;

		// Token: 0x040001BF RID: 447
		public const int MaxMaxHp = 9999;

		// Token: 0x040001C0 RID: 448
		public const int MaxBlock = 9999;

		// Token: 0x040001C1 RID: 449
		public const int MaxShield = 9999;

		// Token: 0x040001C2 RID: 450
		public const int MaxMoney = 99999;

		// Token: 0x040001C3 RID: 451
		public const int MaxManaComponentValue = 99;

		// Token: 0x040001C4 RID: 452
		public const int MaxCounter = 999;

		// Token: 0x040001C5 RID: 453
		public const int MaxUpgradeTimes = 99;

		// Token: 0x040001C6 RID: 454
		public const int MaxLoyalty = 9;

		// Token: 0x040001C7 RID: 455
		public const int MaxApplyingLevel = 9999;

		// Token: 0x040001C8 RID: 456
		public const int MaxLevel = 999;

		// Token: 0x040001C9 RID: 457
		public const int MaxApplyingDuration = 9999;

		// Token: 0x040001CA RID: 458
		public const int MaxDuration = 999;

		// Token: 0x040001CB RID: 459
		public const int MaxApplyingCount = 9999;

		// Token: 0x040001CC RID: 460
		public const int MaxCount = 999;

		// Token: 0x040001CD RID: 461
		public const int ShopNormalCardCount = 8;

		// Token: 0x040001CE RID: 462
		public const int ShopToolCardCount = 2;

		// Token: 0x040001CF RID: 463
		public const int ShopExhibitCount = 3;

		// Token: 0x040001D0 RID: 464
		public const int FriendWeight = 5;

		// Token: 0x040001D1 RID: 465
		public const int ShopUpgradeCardPrice = 50;

		// Token: 0x040001D2 RID: 466
		public const int ShopRemoveCardPrice = 50;

		// Token: 0x040001D3 RID: 467
		public const int ShopRemoveCardPriceIncrement = 25;

		// Token: 0x040001D4 RID: 468
		public static readonly int[] CardPrices = new int[] { 50, 75, 150 };

		// Token: 0x040001D5 RID: 469
		public static readonly int[] ExhibitPrices = new int[] { 150, 200, 300 };

		// Token: 0x040001D6 RID: 470
		public static readonly int[] ToolPrices = new int[] { 50, 70, 100 };

		// Token: 0x040001D7 RID: 471
		public const float PriceRange = 8f;

		// Token: 0x040001D8 RID: 472
		public const int OpponentCandidateCount = 3;

		// Token: 0x040001D9 RID: 473
		public const float TurboModeTimeScale = 2f;

		// Token: 0x040001DA RID: 474
		public const float DefaultAnimationWaitTime = 0.2f;

		// Token: 0x040001DB RID: 475
		public const float DefaultSeWaitTime = 0.2f;

		// Token: 0x040001DC RID: 476
		public const float BaseTimeStep = 0.016666668f;

		// Token: 0x040001DD RID: 477
		public const int BaseTickPerSecond = 60;

		// Token: 0x040001DE RID: 478
		public const int PieceInGun = 100;

		// Token: 0x040001DF RID: 479
		public const float GunMaxTime = 10f;

		// Token: 0x040001E0 RID: 480
		public const float ShootMaxTime = 5f;

		// Token: 0x040001E1 RID: 481
		public const float ShootMinTime = 0.1f;

		// Token: 0x040001E2 RID: 482
		public const float ShootEndAnimationEarliestTime = 0.5f;

		// Token: 0x040001E3 RID: 483
		public const float ShooterLoopMinTime = 0.3f;

		// Token: 0x040001E4 RID: 484
		public const float ShooterEndTime = 0.3f;

		// Token: 0x040001E5 RID: 485
		public const float CrashTime = 0.3f;

		// Token: 0x040001E6 RID: 486
		public const float GunForceEndTime = 0.1f;

		// Token: 0x040001E7 RID: 487
		public const float SpellDeclareTime = 2f;

		// Token: 0x040001E8 RID: 488
		public const int HealLargeFloor = 12;

		// Token: 0x040001E9 RID: 489
		public const int GapDefaultHealRatio = 30;

		// Token: 0x040001EA RID: 490
		public const int RewardCardAbandonMoneyInitial = 5;

		// Token: 0x040001EB RID: 491
		public static readonly int[] EnemyStationMoney = new int[] { 10, 20 };

		// Token: 0x040001EC RID: 492
		public static readonly int[] EnemyStationMoneyHigh = new int[] { 25, 35 };

		// Token: 0x040001ED RID: 493
		public static readonly int[] EliteStationMoney = new int[] { 30, 40 };

		// Token: 0x040001EE RID: 494
		public static readonly int[] BossStationMoney = new int[] { 95, 105 };

		// Token: 0x040001EF RID: 495
		public static readonly Color DefaultGeneralColor = new Color32(82, 112, 239, byte.MaxValue);

		// Token: 0x040001F0 RID: 496
		public static readonly Color DefaultKeywordColor = new Color32(239, 200, 81, byte.MaxValue);

		// Token: 0x040001F1 RID: 497
		public static readonly Color EntityColor = new Color(0.6f, 1f, 1f, 1f);

		// Token: 0x040001F2 RID: 498
		public static readonly Color NormalColor = new Color(0.7f, 1f, 1f, 1f);

		// Token: 0x040001F3 RID: 499
		public static readonly Color IncreaseColor = new Color(1f, 0.58f, 0f);

		// Token: 0x040001F4 RID: 500
		public static readonly Color DecreaseColor = new Color(1f, 0.6f, 1f);

		// Token: 0x040001F5 RID: 501
		public static readonly Color UiRed = new Color32(221, 116, 115, byte.MaxValue);

		// Token: 0x040001F6 RID: 502
		public static readonly Color UiGreen = new Color32(139, 200, 138, byte.MaxValue);

		// Token: 0x040001F7 RID: 503
		public static readonly Color UiBlue = new Color32(138, 181, 200, byte.MaxValue);

		// Token: 0x040001F8 RID: 504
		public static readonly Color UpgradedGreen = new Color32(95, byte.MaxValue, 153, byte.MaxValue);

		// Token: 0x040001F9 RID: 505
		public static readonly Color GameResultFail = new Color(0.7f, 0f, 0f, 1f);

		// Token: 0x040001FA RID: 506
		public static readonly Color GameResultNormal = new Color(0.7f, 0.7f, 0.9f, 1f);

		// Token: 0x040001FB RID: 507
		public static readonly Color GameResultTrue = new Color(1f, 1f, 0.8f, 1f);

		// Token: 0x040001FC RID: 508
		public static readonly IReadOnlyDictionary<char, Color> ColorCodeTable;
	}
}
