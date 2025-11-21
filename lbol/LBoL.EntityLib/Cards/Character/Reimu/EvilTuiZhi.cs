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
	// Token: 0x020003D0 RID: 976
	[UsedImplicitly]
	public sealed class EvilTuiZhi : Card
	{
		// Token: 0x06000DBC RID: 3516 RVA: 0x00019AC8 File Offset: 0x00017CC8
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<EvilTuiZhiSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
