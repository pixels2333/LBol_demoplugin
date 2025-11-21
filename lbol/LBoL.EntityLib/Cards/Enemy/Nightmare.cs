using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;

namespace LBoL.EntityLib.Cards.Enemy
{
	// Token: 0x02000369 RID: 873
	[UsedImplicitly]
	public sealed class Nightmare : Card
	{
		// Token: 0x17000166 RID: 358
		// (get) Token: 0x06000C8C RID: 3212 RVA: 0x000184FE File Offset: 0x000166FE
		[UsedImplicitly]
		public int NightmareCount
		{
			get
			{
				if (base.Battle != null)
				{
					return Enumerable.Count<Card>(base.Battle.EnumerateAllCardsButExile(), (Card card) => card is Nightmare);
				}
				return 0;
			}
		}

		// Token: 0x06000C8D RID: 3213 RVA: 0x0001853C File Offset: 0x0001673C
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new EventSequencedReactor<CardsEventArgs>(this.OnCardAdded));
			base.ReactBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new EventSequencedReactor<CardsEventArgs>(this.OnCardAdded));
			base.ReactBattleEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new EventSequencedReactor<CardsAddingToDrawZoneEventArgs>(this.OnCardAddedToDraw));
		}

		// Token: 0x06000C8E RID: 3214 RVA: 0x000185A0 File Offset: 0x000167A0
		private IEnumerable<BattleAction> OnCardAddedToDraw(CardsAddingToDrawZoneEventArgs args)
		{
			return this.CheckCard();
		}

		// Token: 0x06000C8F RID: 3215 RVA: 0x000185A8 File Offset: 0x000167A8
		private IEnumerable<BattleAction> OnCardAdded(CardsEventArgs args)
		{
			return this.CheckCard();
		}

		// Token: 0x06000C90 RID: 3216 RVA: 0x000185B0 File Offset: 0x000167B0
		public IEnumerable<BattleAction> CheckCard()
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (this.NightmareCount >= base.Value1)
			{
				List<Card> nightmares = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.EnumerateAllCardsButExile(), (Card card) => card is Nightmare));
				if (this == Enumerable.First<Card>(nightmares))
				{
					Doremy doremy = (Doremy)Enumerable.FirstOrDefault<EnemyUnit>(base.Battle.AllAliveEnemies, (EnemyUnit enemy) => enemy is Doremy);
					if (doremy != null)
					{
						foreach (BattleAction battleAction in doremy.OnNightmareHappened())
						{
							yield return battleAction;
						}
						IEnumerator<BattleAction> enumerator = null;
						yield return new ExileManyCardAction(nightmares);
						yield return new ForceKillAction(doremy, base.Battle.Player);
					}
					else
					{
						yield return new ExileManyCardAction(nightmares);
						yield return new ForceKillAction(base.Battle.Player, base.Battle.Player);
					}
					doremy = null;
				}
				nightmares = null;
			}
			yield break;
			yield break;
		}
	}
}
