using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Neutral.Black;
namespace LBoL.EntityLib.Cards.Neutral.Black
{
	[UsedImplicitly]
	public sealed class TrueMoon : Card
	{
		public override ManaGroup? PlentifulMana
		{
			get
			{
				return new ManaGroup?(base.Mana);
			}
		}
		protected override string GetBaseDescription()
		{
			if (!base.PlentifulHappenThisTurn)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(false);
			yield return base.BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);
			yield return base.BuffAction<UseCardToLoseGame>(0, 0, 0, base.Value2, 0.2f);
			yield break;
		}
	}
}
