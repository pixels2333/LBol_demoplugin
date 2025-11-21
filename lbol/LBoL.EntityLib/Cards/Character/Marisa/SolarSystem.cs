using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Marisa;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000447 RID: 1095
	[UsedImplicitly]
	public sealed class SolarSystem : Card
	{
		// Token: 0x06000EEA RID: 3818 RVA: 0x0001B117 File Offset: 0x00019317
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<SolarSystemSe>(base.Battle.Player, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
