using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002AB RID: 683
	[UsedImplicitly]
	public sealed class SeeFengshui : Card
	{
		// Token: 0x06000A92 RID: 2706 RVA: 0x00015D99 File Offset: 0x00013F99
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<SeeFengshuiSe>(base.Battle.Player, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
