using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.Exhibits.Seija;
using UnityEngine;
namespace LBoL.EntityLib.StatusEffects.Enemy.Seija
{
	public sealed class MadokaBowSe : SeijaSe
	{
		protected override Type ExhibitType
		{
			get
			{
				return typeof(MadokaBow);
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnded));
			this.React(PerformAction.Sfx("GuirenItem", 0f));
			this.React(PerformAction.EffectMessage(unit, "SeijaExhibitManager", "AddExhibit", this));
		}
		private IEnumerable<BattleAction> OnOwnerTurnEnded(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			int num = base.Level / 2;
			int num2 = num;
			int discardCount = num;
			if (base.Level % 2 != 0)
			{
				if (base.GameRun.EnemyBattleRng.NextFloat(0f, 1f) < 0.5f)
				{
					num2++;
				}
				else
				{
					int num3 = discardCount + 1;
					discardCount = num3;
				}
			}
			if (num2 > 3 || discardCount > 3)
			{
				Debug.LogWarning("Too high level for MadokaBow.");
			}
			RepeatableRandomPool<Type> repeatableRandomPool = this._pool1;
			Seija seija = base.Owner as Seija;
			if (seija != null && seija.ItemCount >= 3)
			{
				repeatableRandomPool = this._pool2;
			}
			RandomPoolEntry<Type>[] array = repeatableRandomPool.SampleManyOrAll(base.Level, base.GameRun.EnemyBattleRng);
			List<Card> cards = Enumerable.ToList<Card>(Enumerable.Select<RandomPoolEntry<Type>, Card>(array, (RandomPoolEntry<Type> type) => Library.CreateCard(type.Elem)));
			if (num2 > 0)
			{
				yield return new AddCardsToDrawZoneAction(Enumerable.Take<Card>(cards, num2), DrawZoneTarget.Random, AddCardsType.Normal);
			}
			if (discardCount > 0)
			{
				yield return new AddCardsToDiscardAction(Enumerable.TakeLast<Card>(cards, discardCount), AddCardsType.Normal);
			}
			yield break;
		}
		private readonly RepeatableRandomPool<Type> _pool1 = new RepeatableRandomPool<Type>
		{
			{
				typeof(Riguang),
				1f
			},
			{
				typeof(Yueguang),
				1f
			},
			{
				typeof(Xuanguang),
				1f
			},
			{
				typeof(Xingguang),
				1f
			}
		};
		private readonly RepeatableRandomPool<Type> _pool2 = new RepeatableRandomPool<Type>
		{
			{
				typeof(Riguang),
				1f
			},
			{
				typeof(Yueguang),
				1f
			},
			{
				typeof(Xuanguang),
				1f
			},
			{
				typeof(Chunguang),
				1f
			}
		};
	}
}
