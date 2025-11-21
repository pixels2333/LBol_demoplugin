using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Marisa;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000452 RID: 1106
	[UsedImplicitly]
	public sealed class WizardStudy : Card
	{
		// Token: 0x06000F04 RID: 3844 RVA: 0x0001B30B File Offset: 0x0001950B
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);
			yield return base.BuffAction<Spirit>(base.Value1, 0, 0, 0, 0.2f);
			yield return base.DebuffAction<ManaFreezed>(base.Battle.Player, base.Mana.Any, 0, 0, 0, true, 0.2f);
			yield break;
		}
	}
}
