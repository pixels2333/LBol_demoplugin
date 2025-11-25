using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	[UsedImplicitly]
	public sealed class ShuimiUnderWater : Card
	{
		public override IEnumerable<BattleAction> OnDiscard(CardZone srcZone)
		{
			yield return new GainManaAction(base.Mana);
			yield break;
		}
		public override IEnumerable<BattleAction> OnExile(CardZone srcZone)
		{
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
