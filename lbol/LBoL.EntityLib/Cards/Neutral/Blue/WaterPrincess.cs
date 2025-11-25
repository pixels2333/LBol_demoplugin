using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Neutral.NoColor;
namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	[UsedImplicitly]
	public sealed class WaterPrincess : Card
	{
		[UsedImplicitly]
		public int Special
		{
			get
			{
				if (base.Battle != null)
				{
					return base.Value1 * Enumerable.Count<Card>(base.Battle.HandZone, (Card card) => card != this);
				}
				return 0;
			}
		}
		protected override int AdditionalShield
		{
			get
			{
				return this.Special;
			}
		}
		protected override string GetBaseDescription()
		{
			if (!base.DebutActive)
			{
				return base.GetExtraDescription1;
			}
			return base.GetBaseDescription();
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			if (base.TriggeredAnyhow)
			{
				yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<UManaCard>() });
			}
			yield break;
		}
	}
}
