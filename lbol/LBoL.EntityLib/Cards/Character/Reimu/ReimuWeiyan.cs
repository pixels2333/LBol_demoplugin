using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003F8 RID: 1016
	[UsedImplicitly]
	public sealed class ReimuWeiyan : Card
	{
		// Token: 0x06000E1F RID: 3615 RVA: 0x0001A272 File Offset: 0x00018472
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			foreach (BattleAction battleAction in base.DebuffAction<Weak>(selector.GetUnits(base.Battle), 0, base.Value1, 0, 0, true, 0.2f))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}
	}
}
