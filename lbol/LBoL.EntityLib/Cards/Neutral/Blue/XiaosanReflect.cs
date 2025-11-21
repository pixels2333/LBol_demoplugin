using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	// Token: 0x02000325 RID: 805
	[UsedImplicitly]
	public sealed class XiaosanReflect : Card
	{
		// Token: 0x06000BD9 RID: 3033 RVA: 0x000177B6 File Offset: 0x000159B6
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<Reflect>(base.Value1, 0, 0, 0, 0.2f);
			if (base.Battle.Player.HasStatusEffect<Reflect>())
			{
				base.Battle.Player.GetStatusEffect<Reflect>().Gun = (this.IsUpgraded ? "反击弹幕B" : "反击弹幕");
			}
			yield break;
		}
	}
}
