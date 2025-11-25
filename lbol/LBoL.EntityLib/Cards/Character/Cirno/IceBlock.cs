using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Cirno;
namespace LBoL.EntityLib.Cards.Character.Cirno
{
	[UsedImplicitly]
	public sealed class IceBlock : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Immune>(0, 1, 0, 0, 0.2f);
			yield return new LoseBlockShieldAction(base.Battle.Player, base.Battle.Player.Block, base.Battle.Player.Shield, false);
			if (base.Battle.Player.HasStatusEffect<Graze>())
			{
				yield return new RemoveStatusEffectAction(base.Battle.Player.GetStatusEffect<Graze>(), true, 0.1f);
			}
			foreach (Card card in base.Battle.HandZone)
			{
				if (!card.IsRetain && !card.Summoned)
				{
					card.IsTempRetain = true;
				}
			}
			yield return new RequestEndPlayerTurnAction();
			yield break;
		}
	}
}
