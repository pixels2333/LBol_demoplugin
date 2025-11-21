using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x0200049A RID: 1178
	[UsedImplicitly]
	public sealed class BlueRose : Card
	{
		// Token: 0x06000FC0 RID: 4032 RVA: 0x0001C0C7 File Offset: 0x0001A2C7
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(false);
			yield return base.AttackAction(selector, null);
			yield break;
		}
	}
}
