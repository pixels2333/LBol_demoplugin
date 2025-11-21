using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002CE RID: 718
	[UsedImplicitly]
	public sealed class MogongAttack : Card
	{
		// Token: 0x1700013D RID: 317
		// (get) Token: 0x06000AEC RID: 2796 RVA: 0x0001648F File Offset: 0x0001468F
		// (set) Token: 0x06000AED RID: 2797 RVA: 0x00016497 File Offset: 0x00014697
		private int CopiedDamage { get; set; }

		// Token: 0x1700013E RID: 318
		// (get) Token: 0x06000AEE RID: 2798 RVA: 0x000164A0 File Offset: 0x000146A0
		// (set) Token: 0x06000AEF RID: 2799 RVA: 0x000164A8 File Offset: 0x000146A8
		private int CopiedBlock { get; set; }

		// Token: 0x1700013F RID: 319
		// (get) Token: 0x06000AF0 RID: 2800 RVA: 0x000164B1 File Offset: 0x000146B1
		protected override int AdditionalDamage
		{
			get
			{
				if (base.Battle == null || this.CopiedDamage <= 1)
				{
					return 0;
				}
				return this.CopiedDamage - 1;
			}
		}

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x06000AF1 RID: 2801 RVA: 0x000164CE File Offset: 0x000146CE
		protected override int AdditionalBlock
		{
			get
			{
				if (base.Battle == null || this.CopiedBlock <= 1)
				{
					return 0;
				}
				return this.CopiedBlock - 1;
			}
		}

		// Token: 0x06000AF2 RID: 2802 RVA: 0x000164EB File Offset: 0x000146EB
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed), (GameEventPriority)0);
		}

		// Token: 0x06000AF3 RID: 2803 RVA: 0x0001650C File Offset: 0x0001470C
		private void OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Zone == CardZone.Hand)
			{
				Card card = args.Card;
				CardConfig config = card.Config;
				if (config.Damage != null && card.RawDamage > 0)
				{
					this.CopiedDamage = Math.Max(args.Card.RawDamage, this.CopiedDamage);
				}
				if (config.Block != null && card.RawBlock > 0)
				{
					this.CopiedBlock = Math.Max(args.Card.RawBlock, this.CopiedBlock);
				}
				if (config.Shield != null && card.RawShield > 0)
				{
					this.CopiedBlock = Math.Max(args.Card.RawShield, this.CopiedBlock);
				}
				this.NotifyChanged();
			}
		}

		// Token: 0x06000AF4 RID: 2804 RVA: 0x000165D6 File Offset: 0x000147D6
		public override void OnLeaveHand()
		{
			base.OnLeaveHand();
			this.CopiedDamage = 0;
			this.CopiedBlock = 0;
			this.NotifyChanged();
		}

		// Token: 0x06000AF5 RID: 2805 RVA: 0x000165F2 File Offset: 0x000147F2
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(false);
			yield return base.AttackAction(selector, null);
			yield break;
		}
	}
}
