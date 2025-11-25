using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	[UsedImplicitly]
	public sealed class LightSpeed : Card
	{
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && base.Battle.TurnDiscardHistory.NotEmpty<Card>();
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.TriggeredAnyhow)
			{
				yield return new GainManaAction(base.Mana);
			}
			yield break;
		}
	}
}
