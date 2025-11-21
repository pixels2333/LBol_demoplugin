using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Neutral.NoColor;
using UnityEngine;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000427 RID: 1063
	[UsedImplicitly]
	public sealed class ManaJungle : Card
	{
		// Token: 0x06000E92 RID: 3730 RVA: 0x0001AA71 File Offset: 0x00018C71
		public override IEnumerable<BattleAction> OnDraw()
		{
			return this.EnterHandReactor(true);
		}

		// Token: 0x06000E93 RID: 3731 RVA: 0x0001AA7A File Offset: 0x00018C7A
		public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			if (dstZone != CardZone.Hand)
			{
				return null;
			}
			return this.EnterHandReactor(true);
		}

		// Token: 0x06000E94 RID: 3732 RVA: 0x0001AA89 File Offset: 0x00018C89
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.Zone == CardZone.Hand)
			{
				base.React(this.EnterHandReactor(true));
			}
		}

		// Token: 0x06000E95 RID: 3733 RVA: 0x0001AAA1 File Offset: 0x00018CA1
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			return this.EnterHandReactor(false);
		}

		// Token: 0x06000E96 RID: 3734 RVA: 0x0001AAAA File Offset: 0x00018CAA
		private IEnumerable<BattleAction> EnterHandReactor(bool ensureInHand = true)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (ensureInHand && base.Zone != CardZone.Hand)
			{
				Debug.LogWarning(this.Name + " is not in hand.");
				yield break;
			}
			yield return new AddCardsToHandAction(Library.CreateCards<PManaCard>(base.Value1, false), AddCardsType.Normal);
			yield return new ExileCardAction(this);
			yield break;
		}
	}
}
