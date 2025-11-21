using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002C3 RID: 707
	[UsedImplicitly]
	public sealed class ChengDefense : Card
	{
		// Token: 0x06000ACE RID: 2766 RVA: 0x000162AF File Offset: 0x000144AF
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			int item = base.Battle.CalculateBlockShield(this, (float)this.Block.Block, 0f, BlockShieldType.Normal).Item1;
			if (item > 0)
			{
				yield return base.BuffAction<NextTurnGainBlock>(item, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
