using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class Burst : StatusEffect
	{
		[UsedImplicitly]
		public ManaGroup Mana { get; set; } = ManaGroup.Philosophies(3);
		[UsedImplicitly]
		public int DamageRate
		{
			get
			{
				if (base.Owner == null || !base.Owner.HasStatusEffect<BurstUpgrade>())
				{
					return 2;
				}
				return 3;
			}
		}
		public override bool ForceNotShowDownText
		{
			get
			{
				return true;
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageDealingEventArgs>(base.Owner.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
			base.HandleOwnerEvent<BlockShieldEventArgs>(base.Owner.BlockShieldGaining, new GameEventHandler<BlockShieldEventArgs>(this.OnBlockShieldGaining));
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnded));
			base.React(this.FirstGain(base.Level));
		}
		protected override void OnRemoving(Unit unit)
		{
			base.React(this.Lose(unit));
		}
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				base.React(this.StackGain(other.Level));
			}
			return flag;
		}
		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			if (args.DamageInfo.DamageType == DamageType.Attack)
			{
				args.DamageInfo = args.DamageInfo.MultiplyBy(this.DamageRate);
				args.AddModifier(this);
				if (args.Cause != ActionCause.OnlyCalculate)
				{
					base.NotifyActivating();
				}
			}
		}
		private void OnBlockShieldGaining(BlockShieldEventArgs args)
		{
			if (args.ActionSource is WhiteLaser)
			{
				args.Shield *= (float)this.DamageRate;
				args.AddModifier(this);
				if (args.Cause != ActionCause.OnlyCalculate)
				{
					base.NotifyActivating();
				}
			}
		}
		private IEnumerable<BattleAction> OnTurnEnded(UnitEventArgs args)
		{
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
		private IEnumerable<BattleAction> FirstGain(int level = 1)
		{
			yield return new GainManaAction(this.Mana * level);
			BurstDrawSe statusEffect = base.Owner.GetStatusEffect<BurstDrawSe>();
			if (statusEffect != null)
			{
				statusEffect.NotifyActivating();
				yield return new DrawManyCardAction(statusEffect.Level);
			}
			if (base.Owner.IsAlive)
			{
				yield return PerformAction.Effect(base.Owner, "MarisaBurstStart", 0f, "MarisaBurst", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return PerformAction.Effect(base.Owner, "MarisaBurstLoop", 0.5f, null, 0f, PerformAction.EffectBehavior.Add, 0f);
			}
			yield break;
		}
		private IEnumerable<BattleAction> StackGain(int level = 1)
		{
			yield return new GainManaAction(this.Mana * level);
			BurstDrawSe statusEffect = base.Owner.GetStatusEffect<BurstDrawSe>();
			if (statusEffect != null)
			{
				statusEffect.NotifyActivating();
				yield return new DrawManyCardAction(statusEffect.Level);
			}
			if (base.Owner.IsAlive)
			{
				yield return PerformAction.Effect(base.Owner, "MarisaBurstStart", 0f, "MarisaBurst", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return PerformAction.Sfx("MarisaBurst", 0f);
			}
			yield break;
		}
		private IEnumerable<BattleAction> Lose(Unit unit)
		{
			if (unit.IsAlive)
			{
				yield return PerformAction.Effect(unit, "MarisaBurstEnd", 0f, "MarisaBurstLose", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return PerformAction.Effect(unit, "MarisaBurstLoop", 0f, null, 0f, PerformAction.EffectBehavior.Remove, 0f);
			}
			yield break;
		}
	}
}
