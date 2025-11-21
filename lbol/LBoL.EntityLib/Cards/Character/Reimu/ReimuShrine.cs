using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Reimu;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003F6 RID: 1014
	[UsedImplicitly]
	public sealed class ReimuShrine : Card
	{
		// Token: 0x06000E1B RID: 3611 RVA: 0x0001A242 File Offset: 0x00018442
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<TempSpirit>(base.Value1, 0, 0, 0, 0.2f);
			yield return base.BuffAction<ReimuShrineSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
