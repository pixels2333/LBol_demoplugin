using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Cirno;
namespace LBoL.EntityLib.StatusEffects.Cirno
{
	[UsedImplicitly]
	public sealed class FrozenThroneSe : StatusEffect
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
			List<IceBolt> list = Enumerable.ToList<IceBolt>(Enumerable.OfType<IceBolt>(base.Battle.HandZone));
			if (list.Count < base.Level)
			{
				base.NotifyActivating();
				yield return new AddCardsToHandAction(Library.CreateCards<IceBolt>(base.Level - list.Count, false), AddCardsType.Normal);
			}
			yield break;
		}
	}
}
