using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003AD RID: 941
	[UsedImplicitly]
	public sealed class SakuyaKiller : Card
	{
		// Token: 0x06000D5D RID: 3421 RVA: 0x0001940A File Offset: 0x0001760A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<SakuyaKillerSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
