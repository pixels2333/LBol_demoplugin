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
	[UsedImplicitly]
	public sealed class ShensheRichang : Card
	{
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
