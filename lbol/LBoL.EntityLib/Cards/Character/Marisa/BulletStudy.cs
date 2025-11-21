using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000411 RID: 1041
	[UsedImplicitly]
	public sealed class BulletStudy : Card
	{
		// Token: 0x06000E5A RID: 3674 RVA: 0x0001A66C File Offset: 0x0001886C
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.Overdrive(base.Value2))
			{
				yield return base.OverdriveAction(base.Value2);
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
