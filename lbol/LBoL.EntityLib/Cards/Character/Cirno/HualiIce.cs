using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;
namespace LBoL.EntityLib.Cards.Character.Cirno
{
	[UsedImplicitly]
	public sealed class HualiIce : Card
	{
		[UsedImplicitly]
		public int Countdown
		{
			get
			{
				if (base.Battle != null)
				{
					return Math.Max(base.Value1 - base.Battle.TurnCardUsageHistory.Count, 0);
				}
				return 0;
			}
		}
		public override bool Triggered
		{
			get
			{
				return this.IsForceCost;
			}
		}
		public override bool IsForceCost
		{
			get
			{
				return base.Battle != null && this.Countdown == 0;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector.GetUnits(base.Battle));
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			foreach (BattleAction battleAction in base.DebuffAction<Cold>(base.Battle.AllAliveEnemies, 0, 0, 0, 0, true, 0.03f))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, delegate(CardUsingEventArgs _)
			{
				if (base.Zone == CardZone.Hand)
				{
					this.NotifyChanged();
				}
			}, (GameEventPriority)0);
		}
	}
}
