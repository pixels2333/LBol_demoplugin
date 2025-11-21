using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x0200039F RID: 927
	[UsedImplicitly]
	public sealed class PrivateVision : Card
	{
		// Token: 0x1700017B RID: 379
		// (get) Token: 0x06000D3B RID: 3387 RVA: 0x000191C1 File Offset: 0x000173C1
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000D3C RID: 3388 RVA: 0x000191C4 File Offset: 0x000173C4
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this));
			if (list.Count == 1)
			{
				this.oneTargetHand = list[0];
			}
			if (list.Count <= 1)
			{
				return null;
			}
			return new SelectHandInteraction(1, 1, list);
		}

		// Token: 0x06000D3D RID: 3389 RVA: 0x0001921C File Offset: 0x0001741C
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				Card card = ((SelectHandInteraction)precondition).SelectedCards[0];
				if (card != null)
				{
					yield return new DiscardAction(card);
				}
			}
			else if (this.oneTargetHand != null)
			{
				yield return new DiscardAction(this.oneTargetHand);
				this.oneTargetHand = null;
			}
			yield return base.BuffAction<Graze>(base.Value1, 0, 0, 0, 0.2f);
			if (base.Value2 > 0)
			{
				yield return base.BuffAction<Reflect>(base.Value2, 0, 0, 0, 0.2f);
				Reflect statusEffect = base.Battle.Player.GetStatusEffect<Reflect>();
				if (statusEffect != null)
				{
					statusEffect.Gun = base.Config.GunName;
				}
			}
			yield break;
		}

		// Token: 0x04000104 RID: 260
		private Card oneTargetHand;
	}
}
