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
	public sealed class YukariFriendSe : StatusEffect
	{
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Empty;
			}
		}
		protected override void OnAdded(Unit unit)
		{
			if (!(unit is PlayerUnit))
			{
				Debug.LogError(this.DebugName + " should not apply to non-player unit.");
			}
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return this.TakeEffect();
			yield break;
		}
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
