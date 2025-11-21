using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using UnityEngine;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x0200029E RID: 670
	[UsedImplicitly]
	public sealed class Panzi : Card
	{
		// Token: 0x06000A70 RID: 2672 RVA: 0x00015B8C File Offset: 0x00013D8C
		public override IEnumerable<BattleAction> OnDraw()
		{
			return this.EnterHandReactor();
		}

		// Token: 0x06000A71 RID: 2673 RVA: 0x00015B94 File Offset: 0x00013D94
		public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			if (dstZone != CardZone.Hand)
			{
				return null;
			}
			return this.EnterHandReactor();
		}

		// Token: 0x06000A72 RID: 2674 RVA: 0x00015BA2 File Offset: 0x00013DA2
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.Zone == CardZone.Hand && !base.IsCopy)
			{
				base.React(this.EnterHandReactor());
			}
		}

		// Token: 0x06000A73 RID: 2675 RVA: 0x00015BC1 File Offset: 0x00013DC1
		private IEnumerable<BattleAction> EnterHandReactor()
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Zone != CardZone.Hand)
			{
				Debug.LogWarning(this.Name + " is not in hand.");
				yield break;
			}
			Card card = base.CloneBattleCard();
			yield return new AddCardsToHandAction(new Card[] { card });
			yield break;
		}
	}
}
