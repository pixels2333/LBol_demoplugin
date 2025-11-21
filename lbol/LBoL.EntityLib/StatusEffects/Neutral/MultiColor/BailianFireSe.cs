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
	// Token: 0x02000055 RID: 85
	public sealed class BailianFireSe : StatusEffect
	{
		// Token: 0x17000019 RID: 25
		// (get) Token: 0x0600011E RID: 286 RVA: 0x0000415D File Offset: 0x0000235D
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Anys(1);
			}
		}

		// Token: 0x0600011F RID: 287 RVA: 0x00004165 File Offset: 0x00002365
		protected override void OnAdded(Unit unit)
		{
			if (!(unit is PlayerUnit))
			{
				Debug.LogError(this.DebugName + " should not apply to non-player unit.");
			}
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x06000120 RID: 288 RVA: 0x000041A1 File Offset: 0x000023A1
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return this.TakeEffect();
			yield break;
		}

		// Token: 0x06000121 RID: 289 RVA: 0x000041B4 File Offset: 0x000023B4
		public BattleAction TakeEffect()
		{
			base.NotifyActivating();
			Card[] array = base.Battle.RollCards(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), base.Level, (CardConfig config) => config.Type == CardType.Skill);
			foreach (Card card in array)
			{
				card.SetTurnCost(this.Mana);
				card.IsEthereal = true;
				card.IsExile = true;
			}
			return new AddCardsToHandAction(array);
		}
	}
}
