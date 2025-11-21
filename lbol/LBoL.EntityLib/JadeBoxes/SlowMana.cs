using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.JadeBoxes
{
	// Token: 0x02000118 RID: 280
	[UsedImplicitly]
	public sealed class SlowMana : JadeBox
	{
		// Token: 0x060003DF RID: 991 RVA: 0x0000ABCC File Offset: 0x00008DCC
		protected override void OnEnterBattle()
		{
			foreach (Card card2 in Enumerable.Where<Card>(base.Battle.EnumerateAllCards(), (Card card) => card.Config.Rarity > Rarity.Common))
			{
				card2.IncreaseBaseCost(ManaGroup.Anys(1));
			}
			base.HandleBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x060003E0 RID: 992 RVA: 0x0000AC64 File Offset: 0x00008E64
		private void OnCardUsed(CardUsingEventArgs args)
		{
			Card card = args.Card;
			if (card.Config.Rarity != Rarity.Common && card.Cost.Amount > 0)
			{
				ManaColor[] array = card.Cost.EnumerateComponents().SampleManyOrAll(base.Value1, base.GameRun.BattleRng);
				card.DecreaseBaseCost(ManaGroup.FromComponents(array));
			}
		}
	}
}
