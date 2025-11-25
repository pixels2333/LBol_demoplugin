using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	[ExhibitInfo(WeighterType = typeof(YuekuangQiang.YuekuangQiangWeighter))]
	public sealed class YuekuangQiang : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardEventArgs>(base.Battle.CardDiscarded, new EventSequencedReactor<CardEventArgs>(this.OnCardDiscarded));
		}
		private IEnumerable<BattleAction> OnCardDiscarded(CardEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			EnemyUnit randomAliveEnemy = base.Battle.RandomAliveEnemy;
			if (randomAliveEnemy != null)
			{
				base.NotifyActivating();
				yield return new DamageAction(base.Battle.Player, randomAliveEnemy, DamageInfo.Reaction((float)base.Value1, false), "ExhYuekuang", GunType.Single);
			}
			yield break;
		}
		private class YuekuangQiangWeighter : IExhibitWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				if (gameRun.Player.HasExhibit<ShanshuoBishou>())
				{
					return 1f;
				}
				if (Enumerable.Any<Card>(gameRun.BaseDeck, (Card card) => card.DiscardCard))
				{
					return 1f;
				}
				return 0f;
			}
		}
	}
}
