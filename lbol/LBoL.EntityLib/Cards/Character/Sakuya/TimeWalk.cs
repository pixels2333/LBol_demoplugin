using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003C1 RID: 961
	[UsedImplicitly]
	public sealed class TimeWalk : Card
	{
		// Token: 0x06000D8C RID: 3468 RVA: 0x000196B2 File Offset: 0x000178B2
		protected override string GetBaseDescription()
		{
			if (!this.Active)
			{
				return base.GetExtraDescription1;
			}
			return base.GetBaseDescription();
		}

		// Token: 0x17000183 RID: 387
		// (get) Token: 0x06000D8D RID: 3469 RVA: 0x000196C9 File Offset: 0x000178C9
		private bool Active
		{
			get
			{
				if (base.Battle != null)
				{
					return !Enumerable.Any<Card>(base.Battle.BattleCardUsageHistory, (Card card) => card is TimeWalk);
				}
				return true;
			}
		}

		// Token: 0x06000D8E RID: 3470 RVA: 0x00019707 File Offset: 0x00017907
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (this.Active)
			{
				yield return PerformAction.Effect(base.Battle.Player, "ExtraTime", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return PerformAction.Sfx("ExtraTurnLaunch", 0f);
				yield return PerformAction.Animation(base.Battle.Player, "spell", 1.6f, null, 0f, -1);
				yield return base.BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
				yield return new RequestEndPlayerTurnAction();
			}
			yield break;
		}
	}
}
