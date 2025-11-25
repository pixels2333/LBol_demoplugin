using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Reimu;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class YinyangXueyin : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.SacrificeAction(base.Value2);
			yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<YinyangCard>() });
			yield return base.BuffAction<YinyangXueyinSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
