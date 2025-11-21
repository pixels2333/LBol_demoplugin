using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	// Token: 0x02000047 RID: 71
	public sealed class RinDrawSe : StatusEffect
	{
		// Token: 0x060000DC RID: 220 RVA: 0x000038E9 File Offset: 0x00001AE9
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardEventArgs>(base.Battle.CardDrawn, new EventSequencedReactor<CardEventArgs>(this.OnCardDrawn));
		}

		// Token: 0x060000DD RID: 221 RVA: 0x00003908 File Offset: 0x00001B08
		private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
		{
			if (args.Card.CardType == CardType.Status)
			{
				base.NotifyActivating();
				yield return new DrawManyCardAction(base.Level);
			}
			yield break;
		}
	}
}
