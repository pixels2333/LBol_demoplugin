using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x02000407 RID: 1031
	[UsedImplicitly]
	public sealed class YinyangCard : YinyangCardBase
	{
		// Token: 0x06000E41 RID: 3649 RVA: 0x0001A4D7 File Offset: 0x000186D7
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return base.DefenseAction(false);
			yield break;
		}
	}
}
