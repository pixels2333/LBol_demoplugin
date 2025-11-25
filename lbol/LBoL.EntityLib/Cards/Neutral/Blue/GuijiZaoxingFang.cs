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
	public sealed class GuijiZaoxingFang : Card
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
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
			foreach (GunPair gunPair in base.CardGuns.GunPairs)
			{
				yield return base.AttackAction(selector, gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			if (base.GameRun.BattleCardRng.NextInt(0, 1) == 0)
			{
				yield return new AddCardsToDiscardAction(new Card[] { Library.CreateCard<GuijiZaoxingYuan>(this.IsUpgraded) });
			}
			else
			{
				yield return new AddCardsToDiscardAction(new Card[] { Library.CreateCard<GuijiZaoxingHu>(this.IsUpgraded) });
			}
			yield break;
			yield break;
		}
	}
}
