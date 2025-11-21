using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.Black
{
	// Token: 0x02000064 RID: 100
	public sealed class UseCardToLoseGame : StatusEffect
	{
		// Token: 0x0600015E RID: 350 RVA: 0x00004ACE File Offset: 0x00002CCE
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x0600015F RID: 351 RVA: 0x00004AED File Offset: 0x00002CED
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (args.Card != base.SourceCard)
			{
				int count = base.Count;
				base.Count = count - 1;
				if (base.Count == 1)
				{
					base.Highlight = true;
				}
				this.NotifyChanged();
				if (base.Count <= 0)
				{
					base.NotifyActivating();
					yield return new DamageAction(base.Owner, base.Owner, DamageInfo.Reaction(9999f, false), "Instant", GunType.Single);
					yield return new RemoveStatusEffectAction(this, true, 0.1f);
				}
			}
			yield break;
		}
	}
}
