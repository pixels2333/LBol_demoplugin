using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Neutral.NoColor;
namespace LBoL.EntityLib.StatusEffects.Marisa
{
	public sealed class LearnAstrologySe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			List<Astrology> list = Enumerable.ToList<Astrology>(Enumerable.OfType<Astrology>(base.Battle.HandZone));
			if (list.Count < base.Level)
			{
				base.NotifyActivating();
				yield return new AddCardsToHandAction(Library.CreateCards<Astrology>(base.Level - list.Count, false), AddCardsType.Normal);
			}
			yield break;
		}
	}
}
