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
	// Token: 0x020004C1 RID: 1217
	[UsedImplicitly]
	public sealed class IceBlock : Card
	{
		// Token: 0x06001027 RID: 4135 RVA: 0x0001C9CE File Offset: 0x0001ABCE
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
