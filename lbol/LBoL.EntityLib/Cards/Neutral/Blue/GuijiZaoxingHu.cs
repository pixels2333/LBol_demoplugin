using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	[UsedImplicitly]
	public sealed class GuijiZaoxingHu : Card
	{
		public override bool RemoveFromBattleAfterPlay
		{
			get
			{
				return true;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return new DrawManyCardAction(base.Value1);
			if (base.GameRun.BattleCardRng.NextInt(0, 1) == 0)
			{
				yield return new AddCardsToDiscardAction(new Card[] { Library.CreateCard<GuijiZaoxingYuan>(this.IsUpgraded) });
			}
			else
			{
				yield return new AddCardsToDiscardAction(new Card[] { Library.CreateCard<GuijiZaoxingFang>(this.IsUpgraded) });
			}
			yield break;
		}
	}
}
