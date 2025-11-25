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
	public sealed class UseCardToLoseGame : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}
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
