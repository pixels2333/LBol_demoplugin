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
	// Token: 0x0200039C RID: 924
	[UsedImplicitly]
	public sealed class MihuanJack : Card
	{
		// Token: 0x06000D32 RID: 3378 RVA: 0x000190DD File Offset: 0x000172DD
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<MihuanJackSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
