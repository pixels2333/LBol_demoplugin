using System;
using LBoL.Base;

namespace LBoL.Core.Randoms
{
	// Token: 0x020000EA RID: 234
	public sealed class CardTypeWeightTable
	{
		// Token: 0x170002E0 RID: 736
		// (get) Token: 0x0600090A RID: 2314 RVA: 0x0001A5E1 File Offset: 0x000187E1
		public float Attack { get; }

		// Token: 0x170002E1 RID: 737
		// (get) Token: 0x0600090B RID: 2315 RVA: 0x0001A5E9 File Offset: 0x000187E9
		public float Defense { get; }

		// Token: 0x170002E2 RID: 738
		// (get) Token: 0x0600090C RID: 2316 RVA: 0x0001A5F1 File Offset: 0x000187F1
		public float Skill { get; }

		// Token: 0x170002E3 RID: 739
		// (get) Token: 0x0600090D RID: 2317 RVA: 0x0001A5F9 File Offset: 0x000187F9
		public float Ability { get; }

		// Token: 0x170002E4 RID: 740
		// (get) Token: 0x0600090E RID: 2318 RVA: 0x0001A601 File Offset: 0x00018801
		public float Status { get; }

		// Token: 0x170002E5 RID: 741
		// (get) Token: 0x0600090F RID: 2319 RVA: 0x0001A609 File Offset: 0x00018809
		public float Misfortune { get; }

		// Token: 0x170002E6 RID: 742
		// (get) Token: 0x06000910 RID: 2320 RVA: 0x0001A611 File Offset: 0x00018811
		public float Tool { get; }

		// Token: 0x170002E7 RID: 743
		// (get) Token: 0x06000911 RID: 2321 RVA: 0x0001A619 File Offset: 0x00018819
		public float Friend { get; }

		// Token: 0x06000912 RID: 2322 RVA: 0x0001A624 File Offset: 0x00018824
		public CardTypeWeightTable(float attack, float defense, float skill, float ability, float friend, float status, float misfortune, float tool)
		{
			this.Attack = attack;
			this.Defense = defense;
			this.Skill = skill;
			this.Ability = ability;
			this.Friend = friend;
			this.Status = status;
			this.Misfortune = misfortune;
			this.Tool = tool;
		}

		// Token: 0x06000913 RID: 2323 RVA: 0x0001A674 File Offset: 0x00018874
		public float WeightFor(CardType cardInfoType)
		{
			float num;
			switch (cardInfoType)
			{
			case CardType.Unknown:
				throw new InvalidOperationException("Try get weight for unknown typed card");
			case CardType.Attack:
				num = this.Attack;
				break;
			case CardType.Defense:
				num = this.Defense;
				break;
			case CardType.Skill:
				num = this.Skill;
				break;
			case CardType.Ability:
				num = this.Ability;
				break;
			case CardType.Friend:
				num = this.Friend;
				break;
			case CardType.Tool:
				num = this.Tool;
				break;
			case CardType.Status:
				num = this.Status;
				break;
			case CardType.Misfortune:
				num = this.Misfortune;
				break;
			default:
				throw new ArgumentOutOfRangeException("cardInfoType", cardInfoType, null);
			}
			return num;
		}

		// Token: 0x040004BE RID: 1214
		public static readonly CardTypeWeightTable AllOnes = new CardTypeWeightTable(1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f);

		// Token: 0x040004BF RID: 1215
		public static readonly CardTypeWeightTable CanBeLoot = new CardTypeWeightTable(1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f);

		// Token: 0x040004C0 RID: 1216
		public static readonly CardTypeWeightTable OnlyAttack = new CardTypeWeightTable(1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

		// Token: 0x040004C1 RID: 1217
		public static readonly CardTypeWeightTable OnlyDefense = new CardTypeWeightTable(0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f);

		// Token: 0x040004C2 RID: 1218
		public static readonly CardTypeWeightTable OnlySkill = new CardTypeWeightTable(0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f);

		// Token: 0x040004C3 RID: 1219
		public static readonly CardTypeWeightTable OnlyAbility = new CardTypeWeightTable(0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f);

		// Token: 0x040004C4 RID: 1220
		public static readonly CardTypeWeightTable OnlyFriend = new CardTypeWeightTable(0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f);

		// Token: 0x040004C5 RID: 1221
		public static readonly CardTypeWeightTable ShopSkillAndFriend = new CardTypeWeightTable(0f, 0f, 1f, 0f, 5f, 0f, 0f, 0f);

		// Token: 0x040004C6 RID: 1222
		public static readonly CardTypeWeightTable OnlyTool = new CardTypeWeightTable(0f, 0f, 0f, 0f, 0f, 0f, 0f, 1f);
	}
}
