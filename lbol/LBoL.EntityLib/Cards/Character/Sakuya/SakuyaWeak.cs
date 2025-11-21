using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003B6 RID: 950
	[UsedImplicitly]
	public sealed class SakuyaWeak : Card
	{
		// Token: 0x06000D74 RID: 3444 RVA: 0x00019551 File Offset: 0x00017751
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DebuffAction<Weak>(selector.GetEnemy(base.Battle), 0, base.Value1, 0, 0, true, 0.2f);
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
