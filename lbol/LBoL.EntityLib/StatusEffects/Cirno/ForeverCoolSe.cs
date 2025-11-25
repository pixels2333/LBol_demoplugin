using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Cirno
{
	[UsedImplicitly]
	public sealed class ForeverCoolSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			foreach (EnemyUnit enemyUnit in base.Battle.AllAliveEnemies)
			{
				base.HandleOwnerEvent<StatusEffectApplyEventArgs>(enemyUnit.StatusEffectAdded, new GameEventHandler<StatusEffectApplyEventArgs>(this.OnEnemyStatusEffectAdded));
			}
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.EnemySpawned, new GameEventHandler<UnitEventArgs>(this.OnEnemySpawned));
		}
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			Card card = args.Card;
			if (card.CardType == CardType.Friend && !card.Summoning)
			{
				base.NotifyActivating();
				EnemyUnit[] array = base.Battle.AllAliveEnemies.SampleManyOrAll(base.Level, base.GameRun.BattleRng);
				foreach (EnemyUnit enemyUnit in array)
				{
					yield return new ApplyStatusEffectAction<Cold>(enemyUnit, default(int?), default(int?), default(int?), default(int?), 0f, true);
				}
				EnemyUnit[] array2 = null;
			}
			yield break;
		}
		private void OnEnemyStatusEffectAdded(StatusEffectApplyEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				return;
			}
			if (args.Effect is Cold)
			{
				List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card.CardType == CardType.Friend && card.Loyalty < 9));
				if (list.Count > 0)
				{
					base.NotifyActivating();
					foreach (Card card2 in list.SampleManyOrAll(base.Level, base.GameRun.BattleRng))
					{
						card2.NotifyActivating();
						card2.Loyalty++;
					}
				}
			}
		}
		private void OnEnemySpawned(UnitEventArgs args)
		{
			base.HandleOwnerEvent<StatusEffectApplyEventArgs>(args.Unit.StatusEffectAdded, new GameEventHandler<StatusEffectApplyEventArgs>(this.OnEnemyStatusEffectAdded));
		}
	}
}
