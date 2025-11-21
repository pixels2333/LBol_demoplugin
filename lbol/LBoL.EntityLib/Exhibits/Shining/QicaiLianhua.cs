using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Others;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000136 RID: 310
	[UsedImplicitly]
	public sealed class QicaiLianhua : ShiningExhibit
	{
		// Token: 0x0600043E RID: 1086 RVA: 0x0000B630 File Offset: 0x00009830
		protected override string GetBaseDescription()
		{
			if (!(this.ColorRecord != ManaGroup.Empty))
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}

		// Token: 0x0600043F RID: 1087 RVA: 0x0000B651 File Offset: 0x00009851
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
			base.ReactBattleEvent<ManaEventArgs>(base.Battle.ManaGained, new EventSequencedReactor<ManaEventArgs>(this.OnManaGained));
		}

		// Token: 0x06000440 RID: 1088 RVA: 0x0000B68D File Offset: 0x0000988D
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			ManaGroup manaGroup = ManaGroup.Single(ManaColors.Colors.Sample(base.GameRun.BattleRng));
			yield return new GainManaAction(manaGroup);
			yield break;
		}

		// Token: 0x06000441 RID: 1089 RVA: 0x0000B69D File Offset: 0x0000989D
		private IEnumerable<BattleAction> OnManaGained(ManaEventArgs args)
		{
			if (this.ColorRecord.Amount >= 7)
			{
				yield break;
			}
			foreach (ManaColor manaColor in ManaColors.WUBRGCP)
			{
				if (this.ColorRecord[manaColor] == 0 && args.Value.GetValue(manaColor) > 0)
				{
					this.ColorRecord[manaColor] = 1;
				}
			}
			if (base.Counter != this.ColorRecord.Amount)
			{
				base.Counter = this.ColorRecord.Amount;
			}
			if (this.ColorRecord.Amount >= 7)
			{
				base.NotifyActivating();
				if (base.Battle.HandIsFull)
				{
					yield return new AddCardsToDrawZoneAction(Library.CreateCards<MeilingLianhua>(1, false), DrawZoneTarget.Top, AddCardsType.Normal);
				}
				else
				{
					yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<MeilingLianhua>() });
				}
			}
			yield break;
		}

		// Token: 0x06000442 RID: 1090 RVA: 0x0000B6B4 File Offset: 0x000098B4
		protected override void OnLeaveBattle()
		{
			this.ColorRecord = ManaGroup.Empty;
		}

		// Token: 0x0400003C RID: 60
		[UsedImplicitly]
		public ManaGroup ColorRecord;
	}
}
