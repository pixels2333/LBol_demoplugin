using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Adventure
{
	[UsedImplicitly]
	public sealed class NewsTidbit : Card
	{
		public override bool Negative
		{
			get
			{
				return true;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DebuffAction<LockedOn>(base.Battle.Player, base.Value1, 0, 0, 0, true, 0.2f);
			yield break;
		}
	}
}
