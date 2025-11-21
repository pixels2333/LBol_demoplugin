using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000010 RID: 16
	public sealed class CardConfig
	{
		// Token: 0x0600021F RID: 543 RVA: 0x0000796C File Offset: 0x00005B6C
		private CardConfig(int Index, string Id, int Order, bool AutoPerform, string[][] Perform, string GunName, string GunNameBurst, int DebugLevel, bool Revealable, bool IsPooled, bool HideMesuem, bool FindInBattle, bool IsUpgradable, Rarity Rarity, CardType Type, TargetType? TargetType, IReadOnlyList<ManaColor> Colors, bool IsXCost, ManaGroup Cost, ManaGroup? UpgradedCost, ManaGroup? Kicker, ManaGroup? UpgradedKicker, int? MoneyCost, int? Damage, int? UpgradedDamage, int? Block, int? UpgradedBlock, int? Shield, int? UpgradedShield, int? Value1, int? UpgradedValue1, int? Value2, int? UpgradedValue2, ManaGroup? Mana, ManaGroup? UpgradedMana, int? Scry, int? UpgradedScry, int? ToolPlayableTimes, int? Loyalty, int? UpgradedLoyalty, int? PassiveCost, int? UpgradedPassiveCost, int? ActiveCost, int? UpgradedActiveCost, int? ActiveCost2, int? UpgradedActiveCost2, int? UltimateCost, int? UpgradedUltimateCost, Keyword Keywords, Keyword UpgradedKeywords, bool EmptyDescription, Keyword RelativeKeyword, Keyword UpgradedRelativeKeyword, IReadOnlyList<string> RelativeEffects, IReadOnlyList<string> UpgradedRelativeEffects, IReadOnlyList<string> RelativeCards, IReadOnlyList<string> UpgradedRelativeCards, string Owner, string ImageId, string UpgradeImageId, bool Unfinished, string Illustrator, IReadOnlyList<string> SubIllustrator)
		{
			this.Index = Index;
			this.Id = Id;
			this.Order = Order;
			this.AutoPerform = AutoPerform;
			this.Perform = Perform;
			this.GunName = GunName;
			this.GunNameBurst = GunNameBurst;
			this.DebugLevel = DebugLevel;
			this.Revealable = Revealable;
			this.IsPooled = IsPooled;
			this.HideMesuem = HideMesuem;
			this.FindInBattle = FindInBattle;
			this.IsUpgradable = IsUpgradable;
			this.Rarity = Rarity;
			this.Type = Type;
			this.TargetType = TargetType;
			this.Colors = Colors;
			this.IsXCost = IsXCost;
			this.Cost = Cost;
			this.UpgradedCost = UpgradedCost;
			this.Kicker = Kicker;
			this.UpgradedKicker = UpgradedKicker;
			this.MoneyCost = MoneyCost;
			this.Damage = Damage;
			this.UpgradedDamage = UpgradedDamage;
			this.Block = Block;
			this.UpgradedBlock = UpgradedBlock;
			this.Shield = Shield;
			this.UpgradedShield = UpgradedShield;
			this.Value1 = Value1;
			this.UpgradedValue1 = UpgradedValue1;
			this.Value2 = Value2;
			this.UpgradedValue2 = UpgradedValue2;
			this.Mana = Mana;
			this.UpgradedMana = UpgradedMana;
			this.Scry = Scry;
			this.UpgradedScry = UpgradedScry;
			this.ToolPlayableTimes = ToolPlayableTimes;
			this.Loyalty = Loyalty;
			this.UpgradedLoyalty = UpgradedLoyalty;
			this.PassiveCost = PassiveCost;
			this.UpgradedPassiveCost = UpgradedPassiveCost;
			this.ActiveCost = ActiveCost;
			this.UpgradedActiveCost = UpgradedActiveCost;
			this.ActiveCost2 = ActiveCost2;
			this.UpgradedActiveCost2 = UpgradedActiveCost2;
			this.UltimateCost = UltimateCost;
			this.UpgradedUltimateCost = UpgradedUltimateCost;
			this.Keywords = Keywords;
			this.UpgradedKeywords = UpgradedKeywords;
			this.EmptyDescription = EmptyDescription;
			this.RelativeKeyword = RelativeKeyword;
			this.UpgradedRelativeKeyword = UpgradedRelativeKeyword;
			this.RelativeEffects = RelativeEffects;
			this.UpgradedRelativeEffects = UpgradedRelativeEffects;
			this.RelativeCards = RelativeCards;
			this.UpgradedRelativeCards = UpgradedRelativeCards;
			this.Owner = Owner;
			this.ImageId = ImageId;
			this.UpgradeImageId = UpgradeImageId;
			this.Unfinished = Unfinished;
			this.Illustrator = Illustrator;
			this.SubIllustrator = SubIllustrator;
		}

		// Token: 0x170000BE RID: 190
		// (get) Token: 0x06000220 RID: 544 RVA: 0x00007B74 File Offset: 0x00005D74
		// (set) Token: 0x06000221 RID: 545 RVA: 0x00007B7C File Offset: 0x00005D7C
		public int Index { get; private set; }

		// Token: 0x170000BF RID: 191
		// (get) Token: 0x06000222 RID: 546 RVA: 0x00007B85 File Offset: 0x00005D85
		// (set) Token: 0x06000223 RID: 547 RVA: 0x00007B8D File Offset: 0x00005D8D
		public string Id { get; private set; }

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x06000224 RID: 548 RVA: 0x00007B96 File Offset: 0x00005D96
		// (set) Token: 0x06000225 RID: 549 RVA: 0x00007B9E File Offset: 0x00005D9E
		public int Order { get; private set; }

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x06000226 RID: 550 RVA: 0x00007BA7 File Offset: 0x00005DA7
		// (set) Token: 0x06000227 RID: 551 RVA: 0x00007BAF File Offset: 0x00005DAF
		public bool AutoPerform { get; private set; }

		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x06000228 RID: 552 RVA: 0x00007BB8 File Offset: 0x00005DB8
		// (set) Token: 0x06000229 RID: 553 RVA: 0x00007BC0 File Offset: 0x00005DC0
		public string[][] Perform { get; private set; }

		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x0600022A RID: 554 RVA: 0x00007BC9 File Offset: 0x00005DC9
		// (set) Token: 0x0600022B RID: 555 RVA: 0x00007BD1 File Offset: 0x00005DD1
		public string GunName { get; private set; }

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x0600022C RID: 556 RVA: 0x00007BDA File Offset: 0x00005DDA
		// (set) Token: 0x0600022D RID: 557 RVA: 0x00007BE2 File Offset: 0x00005DE2
		public string GunNameBurst { get; private set; }

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x0600022E RID: 558 RVA: 0x00007BEB File Offset: 0x00005DEB
		// (set) Token: 0x0600022F RID: 559 RVA: 0x00007BF3 File Offset: 0x00005DF3
		public int DebugLevel { get; private set; }

		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x06000230 RID: 560 RVA: 0x00007BFC File Offset: 0x00005DFC
		// (set) Token: 0x06000231 RID: 561 RVA: 0x00007C04 File Offset: 0x00005E04
		public bool Revealable { get; private set; }

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x06000232 RID: 562 RVA: 0x00007C0D File Offset: 0x00005E0D
		// (set) Token: 0x06000233 RID: 563 RVA: 0x00007C15 File Offset: 0x00005E15
		public bool IsPooled { get; private set; }

		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x06000234 RID: 564 RVA: 0x00007C1E File Offset: 0x00005E1E
		// (set) Token: 0x06000235 RID: 565 RVA: 0x00007C26 File Offset: 0x00005E26
		public bool HideMesuem { get; private set; }

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x06000236 RID: 566 RVA: 0x00007C2F File Offset: 0x00005E2F
		// (set) Token: 0x06000237 RID: 567 RVA: 0x00007C37 File Offset: 0x00005E37
		public bool FindInBattle { get; private set; }

		// Token: 0x170000CA RID: 202
		// (get) Token: 0x06000238 RID: 568 RVA: 0x00007C40 File Offset: 0x00005E40
		// (set) Token: 0x06000239 RID: 569 RVA: 0x00007C48 File Offset: 0x00005E48
		public bool IsUpgradable { get; private set; }

		// Token: 0x170000CB RID: 203
		// (get) Token: 0x0600023A RID: 570 RVA: 0x00007C51 File Offset: 0x00005E51
		// (set) Token: 0x0600023B RID: 571 RVA: 0x00007C59 File Offset: 0x00005E59
		public Rarity Rarity { get; private set; }

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x0600023C RID: 572 RVA: 0x00007C62 File Offset: 0x00005E62
		// (set) Token: 0x0600023D RID: 573 RVA: 0x00007C6A File Offset: 0x00005E6A
		public CardType Type { get; private set; }

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x0600023E RID: 574 RVA: 0x00007C73 File Offset: 0x00005E73
		// (set) Token: 0x0600023F RID: 575 RVA: 0x00007C7B File Offset: 0x00005E7B
		public TargetType? TargetType { get; private set; }

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x06000240 RID: 576 RVA: 0x00007C84 File Offset: 0x00005E84
		// (set) Token: 0x06000241 RID: 577 RVA: 0x00007C8C File Offset: 0x00005E8C
		public IReadOnlyList<ManaColor> Colors { get; private set; }

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x06000242 RID: 578 RVA: 0x00007C95 File Offset: 0x00005E95
		// (set) Token: 0x06000243 RID: 579 RVA: 0x00007C9D File Offset: 0x00005E9D
		public bool IsXCost { get; private set; }

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x06000244 RID: 580 RVA: 0x00007CA6 File Offset: 0x00005EA6
		// (set) Token: 0x06000245 RID: 581 RVA: 0x00007CAE File Offset: 0x00005EAE
		public ManaGroup Cost { get; private set; }

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x06000246 RID: 582 RVA: 0x00007CB7 File Offset: 0x00005EB7
		// (set) Token: 0x06000247 RID: 583 RVA: 0x00007CBF File Offset: 0x00005EBF
		public ManaGroup? UpgradedCost { get; private set; }

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x06000248 RID: 584 RVA: 0x00007CC8 File Offset: 0x00005EC8
		// (set) Token: 0x06000249 RID: 585 RVA: 0x00007CD0 File Offset: 0x00005ED0
		public ManaGroup? Kicker { get; private set; }

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x0600024A RID: 586 RVA: 0x00007CD9 File Offset: 0x00005ED9
		// (set) Token: 0x0600024B RID: 587 RVA: 0x00007CE1 File Offset: 0x00005EE1
		public ManaGroup? UpgradedKicker { get; private set; }

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x0600024C RID: 588 RVA: 0x00007CEA File Offset: 0x00005EEA
		// (set) Token: 0x0600024D RID: 589 RVA: 0x00007CF2 File Offset: 0x00005EF2
		public int? MoneyCost { get; private set; }

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x0600024E RID: 590 RVA: 0x00007CFB File Offset: 0x00005EFB
		// (set) Token: 0x0600024F RID: 591 RVA: 0x00007D03 File Offset: 0x00005F03
		public int? Damage { get; private set; }

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x06000250 RID: 592 RVA: 0x00007D0C File Offset: 0x00005F0C
		// (set) Token: 0x06000251 RID: 593 RVA: 0x00007D14 File Offset: 0x00005F14
		public int? UpgradedDamage { get; private set; }

		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x06000252 RID: 594 RVA: 0x00007D1D File Offset: 0x00005F1D
		// (set) Token: 0x06000253 RID: 595 RVA: 0x00007D25 File Offset: 0x00005F25
		public int? Block { get; private set; }

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x06000254 RID: 596 RVA: 0x00007D2E File Offset: 0x00005F2E
		// (set) Token: 0x06000255 RID: 597 RVA: 0x00007D36 File Offset: 0x00005F36
		public int? UpgradedBlock { get; private set; }

		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x06000256 RID: 598 RVA: 0x00007D3F File Offset: 0x00005F3F
		// (set) Token: 0x06000257 RID: 599 RVA: 0x00007D47 File Offset: 0x00005F47
		public int? Shield { get; private set; }

		// Token: 0x170000DA RID: 218
		// (get) Token: 0x06000258 RID: 600 RVA: 0x00007D50 File Offset: 0x00005F50
		// (set) Token: 0x06000259 RID: 601 RVA: 0x00007D58 File Offset: 0x00005F58
		public int? UpgradedShield { get; private set; }

		// Token: 0x170000DB RID: 219
		// (get) Token: 0x0600025A RID: 602 RVA: 0x00007D61 File Offset: 0x00005F61
		// (set) Token: 0x0600025B RID: 603 RVA: 0x00007D69 File Offset: 0x00005F69
		public int? Value1 { get; private set; }

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x0600025C RID: 604 RVA: 0x00007D72 File Offset: 0x00005F72
		// (set) Token: 0x0600025D RID: 605 RVA: 0x00007D7A File Offset: 0x00005F7A
		public int? UpgradedValue1 { get; private set; }

		// Token: 0x170000DD RID: 221
		// (get) Token: 0x0600025E RID: 606 RVA: 0x00007D83 File Offset: 0x00005F83
		// (set) Token: 0x0600025F RID: 607 RVA: 0x00007D8B File Offset: 0x00005F8B
		public int? Value2 { get; private set; }

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x06000260 RID: 608 RVA: 0x00007D94 File Offset: 0x00005F94
		// (set) Token: 0x06000261 RID: 609 RVA: 0x00007D9C File Offset: 0x00005F9C
		public int? UpgradedValue2 { get; private set; }

		// Token: 0x170000DF RID: 223
		// (get) Token: 0x06000262 RID: 610 RVA: 0x00007DA5 File Offset: 0x00005FA5
		// (set) Token: 0x06000263 RID: 611 RVA: 0x00007DAD File Offset: 0x00005FAD
		public ManaGroup? Mana { get; private set; }

		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x06000264 RID: 612 RVA: 0x00007DB6 File Offset: 0x00005FB6
		// (set) Token: 0x06000265 RID: 613 RVA: 0x00007DBE File Offset: 0x00005FBE
		public ManaGroup? UpgradedMana { get; private set; }

		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x06000266 RID: 614 RVA: 0x00007DC7 File Offset: 0x00005FC7
		// (set) Token: 0x06000267 RID: 615 RVA: 0x00007DCF File Offset: 0x00005FCF
		public int? Scry { get; private set; }

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x06000268 RID: 616 RVA: 0x00007DD8 File Offset: 0x00005FD8
		// (set) Token: 0x06000269 RID: 617 RVA: 0x00007DE0 File Offset: 0x00005FE0
		public int? UpgradedScry { get; private set; }

		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x0600026A RID: 618 RVA: 0x00007DE9 File Offset: 0x00005FE9
		// (set) Token: 0x0600026B RID: 619 RVA: 0x00007DF1 File Offset: 0x00005FF1
		public int? ToolPlayableTimes { get; private set; }

		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x0600026C RID: 620 RVA: 0x00007DFA File Offset: 0x00005FFA
		// (set) Token: 0x0600026D RID: 621 RVA: 0x00007E02 File Offset: 0x00006002
		public int? Loyalty { get; private set; }

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x0600026E RID: 622 RVA: 0x00007E0B File Offset: 0x0000600B
		// (set) Token: 0x0600026F RID: 623 RVA: 0x00007E13 File Offset: 0x00006013
		public int? UpgradedLoyalty { get; private set; }

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x06000270 RID: 624 RVA: 0x00007E1C File Offset: 0x0000601C
		// (set) Token: 0x06000271 RID: 625 RVA: 0x00007E24 File Offset: 0x00006024
		public int? PassiveCost { get; private set; }

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x06000272 RID: 626 RVA: 0x00007E2D File Offset: 0x0000602D
		// (set) Token: 0x06000273 RID: 627 RVA: 0x00007E35 File Offset: 0x00006035
		public int? UpgradedPassiveCost { get; private set; }

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x06000274 RID: 628 RVA: 0x00007E3E File Offset: 0x0000603E
		// (set) Token: 0x06000275 RID: 629 RVA: 0x00007E46 File Offset: 0x00006046
		public int? ActiveCost { get; private set; }

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x06000276 RID: 630 RVA: 0x00007E4F File Offset: 0x0000604F
		// (set) Token: 0x06000277 RID: 631 RVA: 0x00007E57 File Offset: 0x00006057
		public int? UpgradedActiveCost { get; private set; }

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x06000278 RID: 632 RVA: 0x00007E60 File Offset: 0x00006060
		// (set) Token: 0x06000279 RID: 633 RVA: 0x00007E68 File Offset: 0x00006068
		public int? ActiveCost2 { get; private set; }

		// Token: 0x170000EB RID: 235
		// (get) Token: 0x0600027A RID: 634 RVA: 0x00007E71 File Offset: 0x00006071
		// (set) Token: 0x0600027B RID: 635 RVA: 0x00007E79 File Offset: 0x00006079
		public int? UpgradedActiveCost2 { get; private set; }

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x0600027C RID: 636 RVA: 0x00007E82 File Offset: 0x00006082
		// (set) Token: 0x0600027D RID: 637 RVA: 0x00007E8A File Offset: 0x0000608A
		public int? UltimateCost { get; private set; }

		// Token: 0x170000ED RID: 237
		// (get) Token: 0x0600027E RID: 638 RVA: 0x00007E93 File Offset: 0x00006093
		// (set) Token: 0x0600027F RID: 639 RVA: 0x00007E9B File Offset: 0x0000609B
		public int? UpgradedUltimateCost { get; private set; }

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x06000280 RID: 640 RVA: 0x00007EA4 File Offset: 0x000060A4
		// (set) Token: 0x06000281 RID: 641 RVA: 0x00007EAC File Offset: 0x000060AC
		public Keyword Keywords { get; private set; }

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x06000282 RID: 642 RVA: 0x00007EB5 File Offset: 0x000060B5
		// (set) Token: 0x06000283 RID: 643 RVA: 0x00007EBD File Offset: 0x000060BD
		public Keyword UpgradedKeywords { get; private set; }

		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x06000284 RID: 644 RVA: 0x00007EC6 File Offset: 0x000060C6
		// (set) Token: 0x06000285 RID: 645 RVA: 0x00007ECE File Offset: 0x000060CE
		public bool EmptyDescription { get; private set; }

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x06000286 RID: 646 RVA: 0x00007ED7 File Offset: 0x000060D7
		// (set) Token: 0x06000287 RID: 647 RVA: 0x00007EDF File Offset: 0x000060DF
		public Keyword RelativeKeyword { get; private set; }

		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x06000288 RID: 648 RVA: 0x00007EE8 File Offset: 0x000060E8
		// (set) Token: 0x06000289 RID: 649 RVA: 0x00007EF0 File Offset: 0x000060F0
		public Keyword UpgradedRelativeKeyword { get; private set; }

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x0600028A RID: 650 RVA: 0x00007EF9 File Offset: 0x000060F9
		// (set) Token: 0x0600028B RID: 651 RVA: 0x00007F01 File Offset: 0x00006101
		public IReadOnlyList<string> RelativeEffects { get; private set; }

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x0600028C RID: 652 RVA: 0x00007F0A File Offset: 0x0000610A
		// (set) Token: 0x0600028D RID: 653 RVA: 0x00007F12 File Offset: 0x00006112
		public IReadOnlyList<string> UpgradedRelativeEffects { get; private set; }

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x0600028E RID: 654 RVA: 0x00007F1B File Offset: 0x0000611B
		// (set) Token: 0x0600028F RID: 655 RVA: 0x00007F23 File Offset: 0x00006123
		public IReadOnlyList<string> RelativeCards { get; private set; }

		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x06000290 RID: 656 RVA: 0x00007F2C File Offset: 0x0000612C
		// (set) Token: 0x06000291 RID: 657 RVA: 0x00007F34 File Offset: 0x00006134
		public IReadOnlyList<string> UpgradedRelativeCards { get; private set; }

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x06000292 RID: 658 RVA: 0x00007F3D File Offset: 0x0000613D
		// (set) Token: 0x06000293 RID: 659 RVA: 0x00007F45 File Offset: 0x00006145
		public string Owner { get; private set; }

		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x06000294 RID: 660 RVA: 0x00007F4E File Offset: 0x0000614E
		// (set) Token: 0x06000295 RID: 661 RVA: 0x00007F56 File Offset: 0x00006156
		public string ImageId { get; private set; }

		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x06000296 RID: 662 RVA: 0x00007F5F File Offset: 0x0000615F
		// (set) Token: 0x06000297 RID: 663 RVA: 0x00007F67 File Offset: 0x00006167
		public string UpgradeImageId { get; private set; }

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x06000298 RID: 664 RVA: 0x00007F70 File Offset: 0x00006170
		// (set) Token: 0x06000299 RID: 665 RVA: 0x00007F78 File Offset: 0x00006178
		public bool Unfinished { get; private set; }

		// Token: 0x170000FB RID: 251
		// (get) Token: 0x0600029A RID: 666 RVA: 0x00007F81 File Offset: 0x00006181
		// (set) Token: 0x0600029B RID: 667 RVA: 0x00007F89 File Offset: 0x00006189
		public string Illustrator { get; private set; }

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x0600029C RID: 668 RVA: 0x00007F92 File Offset: 0x00006192
		// (set) Token: 0x0600029D RID: 669 RVA: 0x00007F9A File Offset: 0x0000619A
		public IReadOnlyList<string> SubIllustrator { get; private set; }

		// Token: 0x0600029E RID: 670 RVA: 0x00007FA3 File Offset: 0x000061A3
		public static IReadOnlyList<CardConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<CardConfig>(CardConfig._data);
		}

		// Token: 0x0600029F RID: 671 RVA: 0x00007FB4 File Offset: 0x000061B4
		public static CardConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			CardConfig cardConfig;
			return (!CardConfig._IdTable.TryGetValue(Id, out cardConfig)) ? null : cardConfig;
		}

		// Token: 0x060002A0 RID: 672 RVA: 0x00007FE0 File Offset: 0x000061E0
		public override string ToString()
		{
			string[] array = new string[127];
			array[0] = "{CardConfig Index=";
			array[1] = ConfigDataManager.System_Int32.ToString(this.Index);
			array[2] = ", Id=";
			array[3] = ConfigDataManager.System_String.ToString(this.Id);
			array[4] = ", Order=";
			array[5] = ConfigDataManager.System_Int32.ToString(this.Order);
			array[6] = ", AutoPerform=";
			array[7] = ConfigDataManager.System_Boolean.ToString(this.AutoPerform);
			array[8] = ", Perform=[";
			array[9] = string.Join(", ", Enumerable.Select<string[], string>(this.Perform, (string[] v2) => "[" + string.Join(", ", Enumerable.Select<string, string>(v2, (string v1) => ConfigDataManager.System_String.ToString(v1))) + "]"));
			array[10] = "], GunName=";
			array[11] = ConfigDataManager.System_String.ToString(this.GunName);
			array[12] = ", GunNameBurst=";
			array[13] = ConfigDataManager.System_String.ToString(this.GunNameBurst);
			array[14] = ", DebugLevel=";
			array[15] = ConfigDataManager.System_Int32.ToString(this.DebugLevel);
			array[16] = ", Revealable=";
			array[17] = ConfigDataManager.System_Boolean.ToString(this.Revealable);
			array[18] = ", IsPooled=";
			array[19] = ConfigDataManager.System_Boolean.ToString(this.IsPooled);
			array[20] = ", HideMesuem=";
			array[21] = ConfigDataManager.System_Boolean.ToString(this.HideMesuem);
			array[22] = ", FindInBattle=";
			array[23] = ConfigDataManager.System_Boolean.ToString(this.FindInBattle);
			array[24] = ", IsUpgradable=";
			array[25] = ConfigDataManager.System_Boolean.ToString(this.IsUpgradable);
			array[26] = ", Rarity=";
			array[27] = this.Rarity.ToString();
			array[28] = ", Type=";
			array[29] = this.Type.ToString();
			array[30] = ", TargetType=";
			array[31] = ((this.TargetType == null) ? "null" : this.TargetType.Value.ToString());
			array[32] = ", Colors=[";
			array[33] = string.Join(", ", Enumerable.Select<ManaColor, string>(this.Colors, (ManaColor v1) => ConfigDataManager.LBoL_Base_ManaColor.ToString(v1)));
			array[34] = "], IsXCost=";
			array[35] = ConfigDataManager.System_Boolean.ToString(this.IsXCost);
			array[36] = ", Cost=";
			array[37] = ConfigDataManager.LBoL_Base_ManaGroup.ToString(this.Cost);
			array[38] = ", UpgradedCost=";
			array[39] = ((this.UpgradedCost == null) ? "null" : ConfigDataManager.LBoL_Base_ManaGroup.ToString(this.UpgradedCost.Value));
			array[40] = ", Kicker=";
			array[41] = ((this.Kicker == null) ? "null" : ConfigDataManager.LBoL_Base_ManaGroup.ToString(this.Kicker.Value));
			array[42] = ", UpgradedKicker=";
			array[43] = ((this.UpgradedKicker == null) ? "null" : ConfigDataManager.LBoL_Base_ManaGroup.ToString(this.UpgradedKicker.Value));
			array[44] = ", MoneyCost=";
			array[45] = ((this.MoneyCost == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.MoneyCost.Value));
			array[46] = ", Damage=";
			array[47] = ((this.Damage == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Damage.Value));
			array[48] = ", UpgradedDamage=";
			array[49] = ((this.UpgradedDamage == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedDamage.Value));
			array[50] = ", Block=";
			array[51] = ((this.Block == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Block.Value));
			array[52] = ", UpgradedBlock=";
			array[53] = ((this.UpgradedBlock == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedBlock.Value));
			array[54] = ", Shield=";
			array[55] = ((this.Shield == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Shield.Value));
			array[56] = ", UpgradedShield=";
			array[57] = ((this.UpgradedShield == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedShield.Value));
			array[58] = ", Value1=";
			array[59] = ((this.Value1 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Value1.Value));
			array[60] = ", UpgradedValue1=";
			array[61] = ((this.UpgradedValue1 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedValue1.Value));
			array[62] = ", Value2=";
			array[63] = ((this.Value2 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Value2.Value));
			array[64] = ", UpgradedValue2=";
			array[65] = ((this.UpgradedValue2 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedValue2.Value));
			array[66] = ", Mana=";
			array[67] = ((this.Mana == null) ? "null" : ConfigDataManager.LBoL_Base_ManaGroup.ToString(this.Mana.Value));
			array[68] = ", UpgradedMana=";
			array[69] = ((this.UpgradedMana == null) ? "null" : ConfigDataManager.LBoL_Base_ManaGroup.ToString(this.UpgradedMana.Value));
			array[70] = ", Scry=";
			array[71] = ((this.Scry == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Scry.Value));
			array[72] = ", UpgradedScry=";
			array[73] = ((this.UpgradedScry == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedScry.Value));
			array[74] = ", ToolPlayableTimes=";
			array[75] = ((this.ToolPlayableTimes == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.ToolPlayableTimes.Value));
			array[76] = ", Loyalty=";
			array[77] = ((this.Loyalty == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Loyalty.Value));
			array[78] = ", UpgradedLoyalty=";
			array[79] = ((this.UpgradedLoyalty == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedLoyalty.Value));
			array[80] = ", PassiveCost=";
			array[81] = ((this.PassiveCost == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.PassiveCost.Value));
			array[82] = ", UpgradedPassiveCost=";
			array[83] = ((this.UpgradedPassiveCost == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedPassiveCost.Value));
			array[84] = ", ActiveCost=";
			array[85] = ((this.ActiveCost == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.ActiveCost.Value));
			array[86] = ", UpgradedActiveCost=";
			array[87] = ((this.UpgradedActiveCost == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedActiveCost.Value));
			array[88] = ", ActiveCost2=";
			array[89] = ((this.ActiveCost2 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.ActiveCost2.Value));
			array[90] = ", UpgradedActiveCost2=";
			array[91] = ((this.UpgradedActiveCost2 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedActiveCost2.Value));
			array[92] = ", UltimateCost=";
			array[93] = ((this.UltimateCost == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UltimateCost.Value));
			array[94] = ", UpgradedUltimateCost=";
			array[95] = ((this.UpgradedUltimateCost == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedUltimateCost.Value));
			array[96] = ", Keywords=";
			array[97] = this.Keywords.ToString();
			array[98] = ", UpgradedKeywords=";
			array[99] = this.UpgradedKeywords.ToString();
			array[100] = ", EmptyDescription=";
			array[101] = ConfigDataManager.System_Boolean.ToString(this.EmptyDescription);
			array[102] = ", RelativeKeyword=";
			array[103] = this.RelativeKeyword.ToString();
			array[104] = ", UpgradedRelativeKeyword=";
			array[105] = this.UpgradedRelativeKeyword.ToString();
			array[106] = ", RelativeEffects=[";
			array[107] = string.Join(", ", Enumerable.Select<string, string>(this.RelativeEffects, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[108] = "], UpgradedRelativeEffects=[";
			array[109] = string.Join(", ", Enumerable.Select<string, string>(this.UpgradedRelativeEffects, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[110] = "], RelativeCards=[";
			array[111] = string.Join(", ", Enumerable.Select<string, string>(this.RelativeCards, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[112] = "], UpgradedRelativeCards=[";
			array[113] = string.Join(", ", Enumerable.Select<string, string>(this.UpgradedRelativeCards, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[114] = "], Owner=";
			array[115] = ConfigDataManager.System_String.ToString(this.Owner);
			array[116] = ", ImageId=";
			array[117] = ConfigDataManager.System_String.ToString(this.ImageId);
			array[118] = ", UpgradeImageId=";
			array[119] = ConfigDataManager.System_String.ToString(this.UpgradeImageId);
			array[120] = ", Unfinished=";
			array[121] = ConfigDataManager.System_Boolean.ToString(this.Unfinished);
			array[122] = ", Illustrator=";
			array[123] = ConfigDataManager.System_String.ToString(this.Illustrator);
			array[124] = ", SubIllustrator=[";
			array[125] = string.Join(", ", Enumerable.Select<string, string>(this.SubIllustrator, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[126] = "]}";
			return string.Concat(array);
		}

		// Token: 0x060002A1 RID: 673 RVA: 0x00008CBC File Offset: 0x00006EBC
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				CardConfig[] array = new CardConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new CardConfig(ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.ReadArray<string[]>(binaryReader, (BinaryReader r2) => ConfigDataManager.ReadArray<string>(r2, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1))), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), (Rarity)binaryReader.ReadInt32(), (CardType)binaryReader.ReadInt32(), (!binaryReader.ReadBoolean()) ? null : new TargetType?((TargetType)binaryReader.ReadInt32()), ConfigDataManager.ReadList<ManaColor>(binaryReader, (BinaryReader r1) => ConfigDataManager.LBoL_Base_ManaColor.ReadFrom(r1)), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.LBoL_Base_ManaGroup.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new ManaGroup?(ConfigDataManager.LBoL_Base_ManaGroup.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new ManaGroup?(ConfigDataManager.LBoL_Base_ManaGroup.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new ManaGroup?(ConfigDataManager.LBoL_Base_ManaGroup.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new ManaGroup?(ConfigDataManager.LBoL_Base_ManaGroup.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new ManaGroup?(ConfigDataManager.LBoL_Base_ManaGroup.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (Keyword)binaryReader.ReadInt64(), (Keyword)binaryReader.ReadInt64(), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), (Keyword)binaryReader.ReadInt64(), (Keyword)binaryReader.ReadInt64(), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)));
				}
				CardConfig._data = array;
				CardConfig._IdTable = Enumerable.ToDictionary<CardConfig, string>(CardConfig._data, (CardConfig elem) => elem.Id);
			}
		}

		// Token: 0x060002A2 RID: 674 RVA: 0x00009438 File Offset: 0x00007638
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/CardConfig");
			if (textAsset != null)
			{
				try
				{
					CardConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load CardConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'CardConfig', please reimport config data");
			}
		}

		// Token: 0x0400014D RID: 333
		private static CardConfig[] _data = Array.Empty<CardConfig>();

		// Token: 0x0400014E RID: 334
		private static Dictionary<string, CardConfig> _IdTable = Enumerable.ToDictionary<CardConfig, string>(CardConfig._data, (CardConfig elem) => elem.Id);
	}
}
