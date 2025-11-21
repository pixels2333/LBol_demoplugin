using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003B8 RID: 952
	[UsedImplicitly]
	public sealed class ShengquanZaiwo : Card
	{
		// Token: 0x06000D77 RID: 3447 RVA: 0x00019578 File Offset: 0x00017778
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
