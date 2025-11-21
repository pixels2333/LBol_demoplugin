using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000497 RID: 1175
	[UsedImplicitly]
	public sealed class YaoguaiLaser : Card
	{
		// Token: 0x06000FB6 RID: 4022 RVA: 0x0001BFF2 File Offset: 0x0001A1F2
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(false);
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new FollowAttackAction(selector, false);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.TriggeredAnyhow)
			{
				yield return new FollowAttackAction(selector, false);
			}
			yield break;
		}
	}
}
