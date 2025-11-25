using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	[UsedImplicitly]
	public sealed class YonglinUpgradeSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card.CanUpgradeAndPositive)).SampleManyOrAll(base.Level, base.GameRun.BattleRng));
			if (list.Count > 0)
			{
				base.NotifyActivating();
				yield return new UpgradeCardsAction(list);
			}
			yield break;
		}
	}
}
