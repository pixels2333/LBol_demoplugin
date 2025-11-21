using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x0200045A RID: 1114
	[UsedImplicitly]
	public sealed class Claustrophobia : Card
	{
		// Token: 0x06000F14 RID: 3860 RVA: 0x0001B3D4 File Offset: 0x000195D4
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.TriggeredAnyhow)
			{
				yield return base.DefenseAction(true);
			}
			else
			{
				yield return base.DefenseAction(this.Block.Block, 0, BlockShieldType.Normal, true);
			}
			yield break;
		}
	}
}
