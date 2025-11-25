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
	[UsedImplicitly]
	public sealed class Lunatic : Card
	{
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
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnded));
		}
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
