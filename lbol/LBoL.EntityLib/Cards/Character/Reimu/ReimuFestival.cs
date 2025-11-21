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
	// Token: 0x020003EE RID: 1006
	[UsedImplicitly]
	public sealed class ReimuFestival : Card
	{
		// Token: 0x06000E02 RID: 3586 RVA: 0x00019FBD File Offset: 0x000181BD
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<TempFirepower>(base.Value1, 0, 0, 0, 0.2f);
			yield return base.BuffAction<ReimuFestivalSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
