using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.Cards.Neutral.White
{
	// Token: 0x02000271 RID: 625
	[UsedImplicitly]
	public sealed class BailianJingang : Card
	{
		// Token: 0x060009F1 RID: 2545 RVA: 0x00015107 File Offset: 0x00013307
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<Reflect>(base.Value1, 0, 0, 0, 0.2f);
			if (base.Battle.Player.HasStatusEffect<Reflect>())
			{
				base.Battle.Player.GetStatusEffect<Reflect>().Gun = (this.IsUpgraded ? "金刚体B" : "金刚体");
			}
			yield break;
		}
	}
}
