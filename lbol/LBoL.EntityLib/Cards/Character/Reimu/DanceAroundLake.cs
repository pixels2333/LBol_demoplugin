using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Reimu;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003CB RID: 971
	[UsedImplicitly]
	public sealed class DanceAroundLake : Card
	{
		// Token: 0x06000DAD RID: 3501 RVA: 0x000199AE File Offset: 0x00017BAE
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<DanceAroundLakeSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
