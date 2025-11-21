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

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x02000338 RID: 824
	[UsedImplicitly]
	public sealed class QingeUpgrade : Card
	{
		// Token: 0x06000C05 RID: 3077 RVA: 0x00017AB1 File Offset: 0x00015CB1
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<DieEventArgs>(base.Battle.EnemyDied, new EventSequencedReactor<DieEventArgs>(this.OnEnemyDied));
		}

		// Token: 0x06000C06 RID: 3078 RVA: 0x00017AD0 File Offset: 0x00015CD0
		private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs args)
		{
			if (args.DieSource == this && !args.Unit.HasStatusEffect<Servant>())
			{
				List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.GameRun.BaseDeck, (Card card) => card.CanUpgradeAndPositive));
				if (list.Count > 0)
				{
					Card card3 = list.Sample(base.GameRun.GameRunEventRng);
					base.GameRun.UpgradeDeckCard(card3, false);
					foreach (Card card2 in base.Battle.EnumerateAllCards())
					{
						if (card2.InstanceId == card3.InstanceId)
						{
							if (card2.CanUpgrade)
							{
								yield return new UpgradeCardAction(card2);
								break;
							}
							break;
						}
					}
					IEnumerator<Card> enumerator = null;
				}
			}
			yield break;
			yield break;
		}
	}
}
