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
	// Token: 0x020003FA RID: 1018
	[UsedImplicitly]
	public sealed class SenluoJiejie : Card
	{
		// Token: 0x06000E23 RID: 3619 RVA: 0x0001A2A9 File Offset: 0x000184A9
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<SenluoJiejieSe>(0, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
