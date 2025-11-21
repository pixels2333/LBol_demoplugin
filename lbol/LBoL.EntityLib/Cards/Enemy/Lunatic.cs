using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Enemy
{
	// Token: 0x02000364 RID: 868
	[UsedImplicitly]
	public sealed class Lunatic : Card
	{
		// Token: 0x17000165 RID: 357
		// (get) Token: 0x06000C83 RID: 3203 RVA: 0x00018457 File Offset: 0x00016657
		[UsedImplicitly]
		public int LunaticCount
		{
			get
			{
				if (base.Battle != null)
				{
					return Enumerable.Count<Card>(base.Battle.EnumerateAllCardsButExile(), (Card card) => card is Lunatic);
				}
				return 0;
			}
		}

		// Token: 0x06000C84 RID: 3204 RVA: 0x00018492 File Offset: 0x00016692
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnded));
		}

		// Token: 0x06000C85 RID: 3205 RVA: 0x000184B6 File Offset: 0x000166B6
		private IEnumerable<BattleAction> OnPlayerTurnEnded(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (this.LunaticCount >= base.Value1)
			{
				List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.EnumerateAllCardsButExile(), (Card card) => card is Lunatic));
				if (this == Enumerable.First<Card>(list))
				{
					yield return new ExileManyCardAction(list);
					yield return DamageAction.LoseLife(base.Battle.Player, base.Value2, "JunkoLunaticHit");
				}
			}
			yield break;
		}
	}
}
