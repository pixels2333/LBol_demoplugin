using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.White
{
	// Token: 0x02000273 RID: 627
	[UsedImplicitly]
	public sealed class HuashanQingjing : Card
	{
		// Token: 0x060009F6 RID: 2550 RVA: 0x00015156 File Offset: 0x00013356
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			if (this.IsUpgraded)
			{
				Vulnerable vulnerable = base.Battle.Player.GetStatusEffect<Vulnerable>();
				if (vulnerable != null)
				{
					yield return PerformAction.Sfx("JingHua", 0f);
					yield return new RemoveStatusEffectAction(vulnerable, true, 0.1f);
				}
				vulnerable = null;
			}
			yield break;
		}
	}
}
