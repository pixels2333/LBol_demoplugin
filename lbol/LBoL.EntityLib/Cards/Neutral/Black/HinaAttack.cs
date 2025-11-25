using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.Black
{
	[UsedImplicitly]
	public sealed class HinaAttack : Card
	{
		protected override int AdditionalValue1
		{
			get
			{
				if (base.GameRun != null)
				{
					return Enumerable.Count<Card>(base.GameRun.BaseDeck, (Card card) => card.CardType == CardType.Misfortune);
				}
				return 0;
			}
		}
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}
	}
}
