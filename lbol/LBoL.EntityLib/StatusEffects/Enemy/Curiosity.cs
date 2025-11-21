using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x02000090 RID: 144
	public sealed class Curiosity : StatusEffect
	{
		// Token: 0x0600020D RID: 525 RVA: 0x00006479 File Offset: 0x00004679
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardPlayed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardPlayed));
		}

		// Token: 0x0600020E RID: 526 RVA: 0x000064B5 File Offset: 0x000046B5
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Battle.Player.IsInTurn && args.Card.CardType == CardType.Ability)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}

		// Token: 0x0600020F RID: 527 RVA: 0x000064CC File Offset: 0x000046CC
		private IEnumerable<BattleAction> OnCardPlayed(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Ability)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}
	}
}
