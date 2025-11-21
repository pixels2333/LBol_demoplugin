using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.EntityLib.StatusEffects.Reimu;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003ED RID: 1005
	[UsedImplicitly]
	public sealed class ReimuEvilTerminator : Card
	{
		// Token: 0x06000E00 RID: 3584 RVA: 0x00019FA5 File Offset: 0x000181A5
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<ReimuEvilTerminatorSe>(base.Mana.Amount, 0, 0, 0, 0.2f);
			yield return base.BuffAction<AmuletForCard>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
