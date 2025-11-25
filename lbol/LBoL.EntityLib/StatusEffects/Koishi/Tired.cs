using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using UnityEngine;
namespace LBoL.EntityLib.StatusEffects.Koishi
{
	[UsedImplicitly]
	public sealed class Tired : StatusEffect
	{
		private bool ThisTurnActivating { get; set; }
		protected override string GetBaseDescription()
		{
			if (!this.ThisTurnActivating)
			{
				return base.ExtraDescription;
			}
			return base.GetBaseDescription();
		}
		protected override void OnAdded(Unit unit)
		{
			if (!(unit is PlayerUnit))
			{
				Debug.LogWarning(this.DebugName + " should not apply to non-player unit.");
				this.React(new RemoveStatusEffectAction(this, true, 0.1f));
				return;
			}
			base.Count = base.Limit;
			this.ThisTurnActivating = false;
			base.ShowCount = false;
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, delegate(UnitEventArgs _)
			{
				if (base.Battle.Player.GetStatusEffect<Tired>() == this)
				{
					this.ThisTurnActivating = true;
					base.ShowCount = true;
					base.Highlight = true;
				}
			});
			base.HandleOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed));
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnded));
		}
		private void OnCardUsed(CardUsingEventArgs args)
		{
			if (this.ThisTurnActivating)
			{
				base.Count = base.Limit - base.Battle.TurnCardUsageHistory.Count;
			}
		}
		public override bool ShouldPreventCardUsage(Card card)
		{
			return this.ThisTurnActivating && base.Count <= 0;
		}
		public override string PreventCardUsageMessage
		{
			get
			{
				return "ErrorChat.Tired".Localize(true);
			}
		}
		private IEnumerable<BattleAction> OnPlayerTurnEnded(UnitEventArgs args)
		{
			if (this.ThisTurnActivating)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
	}
}
