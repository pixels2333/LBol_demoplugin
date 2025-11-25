using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	[UsedImplicitly]
	public sealed class PerfectServantWSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			if (base.Battle.HandZone.NotEmpty<Card>())
			{
				foreach (Card card in base.Battle.HandZone)
				{
					if (!(card.Cost == ManaGroup.Empty))
					{
						ManaColor[] array = card.Cost.EnumerateComponents().SampleManyOrAll(base.Level, base.GameRun.BattleRng);
						card.DecreaseTurnCost(ManaGroup.FromComponents(array));
					}
				}
			}
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
