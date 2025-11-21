using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.White
{
	// Token: 0x02000282 RID: 642
	[UsedImplicitly]
	public sealed class YukariSwarm : Card
	{
		// Token: 0x1700012B RID: 299
		// (get) Token: 0x06000A21 RID: 2593 RVA: 0x000154FB File Offset: 0x000136FB
		private Guns Guns1
		{
			get
			{
				return new Guns(base.Config.GunName);
			}
		}

		// Token: 0x1700012C RID: 300
		// (get) Token: 0x06000A22 RID: 2594 RVA: 0x0001550D File Offset: 0x0001370D
		private Guns Guns2
		{
			get
			{
				return new Guns(new string[]
				{
					base.Config.GunName,
					base.Config.GunNameBurst
				});
			}
		}

		// Token: 0x1700012D RID: 301
		// (get) Token: 0x06000A23 RID: 2595 RVA: 0x00015536 File Offset: 0x00013736
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && base.Battle.ExileZone.Count >= base.Value1;
			}
		}

		// Token: 0x06000A24 RID: 2596 RVA: 0x00015560 File Offset: 0x00013760
		public override Interaction Precondition()
		{
			if (base.Value2 <= 0)
			{
				return null;
			}
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card != this));
			if (!list.Empty<Card>())
			{
				return new SelectHandInteraction(0, base.Value2, list);
			}
			return null;
		}

		// Token: 0x06000A25 RID: 2597 RVA: 0x000155B1 File Offset: 0x000137B1
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				IReadOnlyList<Card> selectedCards = ((SelectHandInteraction)precondition).SelectedCards;
				if (selectedCards.Count > 0)
				{
					yield return new ExileManyCardAction(selectedCards);
				}
			}
			base.CardGuns = ((base.Battle.ExileZone.Count >= base.Value1) ? this.Guns2 : this.Guns1);
			foreach (GunPair gunPair in base.CardGuns.GunPairs)
			{
				yield return base.AttackAction(selector, gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			yield break;
			yield break;
		}
	}
}
