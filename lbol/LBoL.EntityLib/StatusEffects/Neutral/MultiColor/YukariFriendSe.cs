using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.EntityLib.StatusEffects.Neutral.MultiColor
{
	// Token: 0x02000058 RID: 88
	public sealed class YukariFriendSe : StatusEffect
	{
		// Token: 0x1700001A RID: 26
		// (get) Token: 0x0600012B RID: 299 RVA: 0x00004393 File Offset: 0x00002593
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Empty;
			}
		}

		// Token: 0x0600012C RID: 300 RVA: 0x0000439A File Offset: 0x0000259A
		protected override void OnAdded(Unit unit)
		{
			if (!(unit is PlayerUnit))
			{
				Debug.LogError(this.DebugName + " should not apply to non-player unit.");
			}
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x0600012D RID: 301 RVA: 0x000043D6 File Offset: 0x000025D6
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return this.TakeEffect();
			yield break;
		}

		// Token: 0x0600012E RID: 302 RVA: 0x000043E8 File Offset: 0x000025E8
		public BattleAction TakeEffect()
		{
			base.NotifyActivating();
			Card[] array = base.Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.OnlyRare, OwnerWeightTable.OnlyNeutral, CardTypeWeightTable.CanBeLoot, false), base.Level, (CardConfig config) => config.Colors.Count == 3 && config.Id != "YukariFriend");
			if (array.Length != 0)
			{
				foreach (Card card in array)
				{
					card.SetTurnCost(this.Mana);
					card.IsEthereal = true;
				}
				return new AddCardsToHandAction(array);
			}
			return null;
		}
	}
}
