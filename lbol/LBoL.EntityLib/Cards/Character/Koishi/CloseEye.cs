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
	// Token: 0x0200045B RID: 1115
	[UsedImplicitly]
	public sealed class CloseEye : Card
	{
		// Token: 0x06000F16 RID: 3862 RVA: 0x0001B3EC File Offset: 0x000195EC
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.DebuffAction<CloseEyeSe>(base.Battle.Player, 1, 0, 0, 0, true, 0.2f);
			yield break;
		}
	}
}
