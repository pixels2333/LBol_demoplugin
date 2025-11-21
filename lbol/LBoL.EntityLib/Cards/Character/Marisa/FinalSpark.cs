using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x0200041D RID: 1053
	[UsedImplicitly]
	public sealed class FinalSpark : Card
	{
		// Token: 0x1700019A RID: 410
		// (get) Token: 0x06000E7B RID: 3707 RVA: 0x0001A91C File Offset: 0x00018B1C
		public override ManaGroup? PlentifulMana
		{
			get
			{
				return new ManaGroup?(base.Mana);
			}
		}

		// Token: 0x06000E7C RID: 3708 RVA: 0x0001A929 File Offset: 0x00018B29
		protected override string GetBaseDescription()
		{
			if (!base.PlentifulHappenThisTurn)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}

		// Token: 0x06000E7D RID: 3709 RVA: 0x0001A940 File Offset: 0x00018B40
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<StatusEffectApplyEventArgs>(base.Battle.Player.StatusEffectAdded, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnStatusEffectAdded));
		}

		// Token: 0x06000E7E RID: 3710 RVA: 0x0001A964 File Offset: 0x00018B64
		private IEnumerable<BattleAction> OnStatusEffectAdded(StatusEffectApplyEventArgs args)
		{
			if (this.IsUpgraded && args.Effect is Burst && base.Zone == CardZone.Discard)
			{
				yield return new MoveCardAction(this, CardZone.Hand);
			}
			yield break;
		}
	}
}
