using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.Colorless.YijiSkills
{
	public sealed class YijiPurify : OptionCard
	{
		public override IEnumerable<BattleAction> TakeEffectActions()
		{
			using (IEnumerator<Card> enumerator = Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => !card.IsPurified).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Card card2 = enumerator.Current;
					card2.NotifyActivating();
					card2.IsPurified = true;
				}
				yield break;
			}
			yield break;
		}
	}
}
