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
	[UsedImplicitly]
	public sealed class JungleMaster : Card
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}
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
