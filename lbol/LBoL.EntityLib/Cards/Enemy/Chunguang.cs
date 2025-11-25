using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Enemy
{
	[UsedImplicitly]
	public sealed class Chunguang : Card
	{
		public override IEnumerable<BattleAction> OnDraw()
		{
			if (base.Battle.BattleMana.HasTrivial)
			{
				yield return ConvertManaAction.Purify(base.Battle.BattleMana, base.Value1);
			}
			yield break;
		}
	}
}
