using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x0200046E RID: 1134
	[UsedImplicitly]
	public sealed class KoishiDna : Card
	{
		// Token: 0x06000F42 RID: 3906 RVA: 0x0001B6E6 File Offset: 0x000198E6
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<KoishiDnaSe>(base.Value1, 0, 0, 0, 0.2f);
			if (this.IsUpgraded)
			{
				KoishiDnaSe statusEffect = base.Battle.Player.GetStatusEffect<KoishiDnaSe>();
				if (statusEffect != null)
				{
					yield return statusEffect.TakeEffect();
				}
			}
			yield break;
		}
	}
}
