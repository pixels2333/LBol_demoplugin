using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	[UsedImplicitly]
	public sealed class SakuyaDraw : Card
	{
		public override IEnumerable<BattleAction> OnDiscard(CardZone srcZone)
		{
			base.Battle.DrawAfterDiscard += base.Value1;
			return base.OnDiscard(srcZone);
		}
		public override IEnumerable<BattleAction> OnExile(CardZone srcZone)
		{
			base.Battle.DrawAfterDiscard += base.Value1;
			return base.OnExile(srcZone);
		}
	}
}
