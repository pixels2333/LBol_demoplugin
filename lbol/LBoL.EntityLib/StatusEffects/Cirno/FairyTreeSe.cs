using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Cirno
{
	// Token: 0x020000DB RID: 219
	[UsedImplicitly]
	public sealed class FairyTreeSe : StatusEffect
	{
		// Token: 0x06000311 RID: 785 RVA: 0x000083E3 File Offset: 0x000065E3
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x06000312 RID: 786 RVA: 0x00008402 File Offset: 0x00006602
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			Card card = args.Card;
			if (card.CardType == CardType.Friend && card.Summoning)
			{
				base.NotifyActivating();
				yield return new CastBlockShieldAction(base.Battle.Player, base.Battle.Player, base.Level, 0, BlockShieldType.Direct, true);
			}
			yield break;
		}
	}
}
