using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003FC RID: 1020
	[UsedImplicitly]
	public sealed class ShensheRichang : Card
	{
		// Token: 0x06000E27 RID: 3623 RVA: 0x0001A2D9 File Offset: 0x000184D9
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.HealAction(base.Value1);
			int block = base.Battle.Player.Block;
			if (block > 0)
			{
				yield return new LoseBlockShieldAction(base.Battle.Player, block, 0, false);
				yield return new CastBlockShieldAction(base.Battle.Player, 0, block, BlockShieldType.Direct, false);
			}
			yield break;
		}
	}
}
