using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003F4 RID: 1012
	[UsedImplicitly]
	public sealed class ReimuLucky : Card
	{
		// Token: 0x06000E14 RID: 3604 RVA: 0x0001A1AB File Offset: 0x000183AB
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Amulet>(base.Value1, 0, 0, 0, 0.2f);
			yield return new GainMoneyAction(base.Value2, SpecialSourceType.None);
			yield break;
		}
	}
}
