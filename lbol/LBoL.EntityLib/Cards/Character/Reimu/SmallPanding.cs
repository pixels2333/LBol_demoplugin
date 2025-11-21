using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003FE RID: 1022
	[UsedImplicitly]
	public sealed class SmallPanding : Card
	{
		// Token: 0x06000E2A RID: 3626 RVA: 0x0001A2F9 File Offset: 0x000184F9
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Graze>(base.Value1, 0, 0, 0, 0.2f);
			if (base.Value2 > 0)
			{
				yield return base.DebuffAction<Vulnerable>(base.Battle.Player, 0, base.Value2, 0, 0, true, 0.2f);
			}
			yield break;
		}
	}
}
