using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.Black
{
	// Token: 0x02000060 RID: 96
	[UsedImplicitly]
	public sealed class DreamExpressSe : StatusEffect
	{
		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000151 RID: 337 RVA: 0x000049D5 File Offset: 0x00002BD5
		private string GunName
		{
			get
			{
				return "CSweet020";
			}
		}

		// Token: 0x06000152 RID: 338 RVA: 0x000049DC File Offset: 0x00002BDC
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}

		// Token: 0x06000153 RID: 339 RVA: 0x00004A00 File Offset: 0x00002C00
		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd || base.Battle.DrawZone.Count == 0)
			{
				yield break;
			}
			base.NotifyActivating();
			DreamCardsAction action = new DreamCardsAction(base.Level, 0);
			yield return action;
			IReadOnlyList<Card> cards = action.Cards;
			if (cards.Count > 0)
			{
				int num = Enumerable.Sum<Card>(cards, (Card card) => card.Config.Cost.Amount);
				if (num > 0)
				{
					yield return new DamageAction(base.Battle.Player, base.Battle.AllAliveEnemies, DamageInfo.Reaction((float)(num * 2), false), this.GunName, GunType.Single);
				}
			}
			yield break;
		}
	}
}
