using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.MultiColor
{
	// Token: 0x020002EA RID: 746
	[UsedImplicitly]
	public sealed class AnimalSpirit : Card
	{
		// Token: 0x06000B2B RID: 2859 RVA: 0x0001695E File Offset: 0x00014B5E
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.Value1 > 0)
			{
				yield return new DrawManyCardAction(base.Value1);
			}
			using (IEnumerator<Card> enumerator = base.Battle.HandZone.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Card card = enumerator.Current;
					List<AnimalSpirit.AnimalType> list = Enumerable.ToList<AnimalSpirit.AnimalType>(this._types);
					if (card.IsEcho || !card.CanBeDuplicated)
					{
						list.Remove(AnimalSpirit.AnimalType.Wolf);
					}
					if (card.IsReplenish)
					{
						list.Remove(AnimalSpirit.AnimalType.Otter);
					}
					if (card.Cost == base.Mana)
					{
						list.Remove(AnimalSpirit.AnimalType.Eagle);
					}
					if (list.Count > 0)
					{
						switch (list.Sample(base.BattleRng))
						{
						case AnimalSpirit.AnimalType.Wolf:
							card.IsEcho = true;
							break;
						case AnimalSpirit.AnimalType.Otter:
							card.IsReplenish = true;
							break;
						case AnimalSpirit.AnimalType.Eagle:
							card.SetTurnCost(base.Mana);
							break;
						default:
							throw new ArgumentOutOfRangeException();
						}
					}
				}
				yield break;
			}
			yield break;
		}

		// Token: 0x06000B2C RID: 2860 RVA: 0x0001696E File Offset: 0x00014B6E
		public AnimalSpirit()
		{
			List<AnimalSpirit.AnimalType> list = new List<AnimalSpirit.AnimalType>();
			list.Add(AnimalSpirit.AnimalType.Wolf);
			list.Add(AnimalSpirit.AnimalType.Otter);
			list.Add(AnimalSpirit.AnimalType.Eagle);
			this._types = list;
			base..ctor();
		}

		// Token: 0x040000F5 RID: 245
		private readonly List<AnimalSpirit.AnimalType> _types;

		// Token: 0x02000837 RID: 2103
		private enum AnimalType
		{
			// Token: 0x04000F2A RID: 3882
			Wolf,
			// Token: 0x04000F2B RID: 3883
			Otter,
			// Token: 0x04000F2C RID: 3884
			Eagle
		}
	}
}
