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

namespace LBoL.EntityLib.Cards.Neutral.Green
{
	// Token: 0x020002F4 RID: 756
	[UsedImplicitly]
	public sealed class AhongMana : Card
	{
		// Token: 0x06000B46 RID: 2886 RVA: 0x00016B99 File Offset: 0x00014D99
		public override IEnumerable<BattleAction> OnDraw()
		{
			return this.EnterHandReactor(true);
		}

		// Token: 0x06000B47 RID: 2887 RVA: 0x00016BA2 File Offset: 0x00014DA2
		public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			if (dstZone != CardZone.Hand)
			{
				return null;
			}
			return this.EnterHandReactor(true);
		}

		// Token: 0x06000B48 RID: 2888 RVA: 0x00016BB1 File Offset: 0x00014DB1
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.Zone == CardZone.Hand)
			{
				base.React(this.EnterHandReactor(true));
			}
		}

		// Token: 0x06000B49 RID: 2889 RVA: 0x00016BC9 File Offset: 0x00014DC9
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			return this.EnterHandReactor(false);
		}

		// Token: 0x06000B4A RID: 2890 RVA: 0x00016BD2 File Offset: 0x00014DD2
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
			yield return new AddCardsToHandAction(Library.CreateCards<GManaCard>(base.Value1, false), AddCardsType.Normal);
			if (base.Value2 > 0)
			{
				yield return base.UpgradeRandomHandAction(base.Value2, CardType.Unknown);
			}
			yield return new ExileCardAction(this);
			yield break;
		}
	}
}
