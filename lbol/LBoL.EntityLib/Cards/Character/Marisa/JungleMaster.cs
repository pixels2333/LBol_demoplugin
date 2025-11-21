using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000425 RID: 1061
	[UsedImplicitly]
	public sealed class JungleMaster : Card
	{
		// Token: 0x06000E8D RID: 3725 RVA: 0x0001AA22 File Offset: 0x00018C22
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}

		// Token: 0x06000E8E RID: 3726 RVA: 0x0001AA41 File Offset: 0x00018C41
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			if (this == Enumerable.FirstOrDefault<Card>(base.Battle.EnumerateAllCards(), (Card card) => card is JungleMaster))
			{
				List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.DrawZone, (Card card) => card is JungleMaster));
				int shield = Enumerable.Sum<Card>(list, (Card card) => card.Value1);
				yield return new ExileManyCardAction(list);
				yield return base.DefenseAction(0, shield, BlockShieldType.Direct, false);
			}
			yield break;
		}
	}
}
