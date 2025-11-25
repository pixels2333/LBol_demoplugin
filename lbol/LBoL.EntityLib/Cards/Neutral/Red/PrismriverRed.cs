using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Enemy;
namespace LBoL.EntityLib.Cards.Neutral.Red
{
	[UsedImplicitly]
	public sealed class PrismriverRed : Card
	{
		[UsedImplicitly]
		public int Light
		{
			get
			{
				return 2;
			}
		}
		protected override string GetBaseDescription()
		{
			if (base.DebutActive || !this.IsUpgraded)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			yield return new DrawManyCardAction(base.Value1);
			yield return new AddCardsToDiscardAction(Library.CreateCards<Xingguang>(this.Light, false), AddCardsType.Normal);
			if (base.TriggeredAnyhow)
			{
				yield return new GainPowerAction(base.Value2);
			}
			yield break;
		}
	}
}
