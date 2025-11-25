using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.ExtraTurn.Partners;
namespace LBoL.EntityLib.Cards.Character.Koishi
{
	[UsedImplicitly]
	public sealed class PsycheLock : Card
	{
		[UsedImplicitly]
		public int Count
		{
			get
			{
				return 3 - this._moods.Count;
			}
		}
		public override bool CanUse
		{
			get
			{
				return this.Count == 0;
			}
		}
		public override bool Triggered
		{
			get
			{
				return this.Count == 0;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return PerformAction.Effect(base.Battle.Player, "ExtraTime", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			yield return PerformAction.Sfx("ExtraTurnLaunch", 0f);
			yield return PerformAction.Animation(base.Battle.Player, "spell", 1.6f, null, 0f, -1);
			yield return base.BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
			if (this.IsUpgraded)
			{
				yield return base.BuffAction<UpgradeAllHandSe>(0, 0, 0, 0, 0.2f);
			}
			yield return new RequestEndPlayerTurnAction();
			yield break;
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<StatusEffectApplyEventArgs>(base.Battle.Player.StatusEffectAdded, new GameEventHandler<StatusEffectApplyEventArgs>(this.OnPlayerSeAdded), (GameEventPriority)0);
			if (base.Zone == CardZone.Hand)
			{
				this.EnterHand();
			}
		}
		private void OnPlayerSeAdded(StatusEffectApplyEventArgs args)
		{
			if (base.Zone == CardZone.Hand)
			{
				Mood mood = args.Effect as Mood;
				if (mood != null && !this._moods.Contains(mood.GetType()))
				{
					this._moods.Add(mood.GetType());
					this.NotifyChanged();
				}
			}
		}
		public override IEnumerable<BattleAction> OnDraw()
		{
			this.EnterHand();
			return null;
		}
		public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			if (dstZone == CardZone.Hand)
			{
				this.EnterHand();
			}
			return null;
		}
		private void EnterHand()
		{
			StatusEffect statusEffect = Enumerable.FirstOrDefault<StatusEffect>(base.Battle.Player.StatusEffects, (StatusEffect se) => se is Mood && !this._moods.Contains(se.GetType()));
			if (statusEffect != null)
			{
				this._moods.Add(statusEffect.GetType());
			}
		}
		public override void OnLeaveHand()
		{
			base.OnLeaveHand();
			this._moods.Clear();
		}
		private readonly List<Type> _moods = new List<Type>();
	}
}
