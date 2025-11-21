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
	// Token: 0x0200008D RID: 141
	[UsedImplicitly]
	public sealed class Burst : StatusEffect
	{
		// Token: 0x1700026B RID: 619
		// (get) Token: 0x06000731 RID: 1841 RVA: 0x00015731 File Offset: 0x00013931
		// (set) Token: 0x06000732 RID: 1842 RVA: 0x00015739 File Offset: 0x00013939
		[UsedImplicitly]
		public ManaGroup Mana { get; set; } = ManaGroup.Philosophies(3);

		// Token: 0x1700026C RID: 620
		// (get) Token: 0x06000733 RID: 1843 RVA: 0x00015742 File Offset: 0x00013942
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

		// Token: 0x1700026D RID: 621
		// (get) Token: 0x06000734 RID: 1844 RVA: 0x0001575C File Offset: 0x0001395C
		public override bool ForceNotShowDownText
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000735 RID: 1845 RVA: 0x00015760 File Offset: 0x00013960
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageDealingEventArgs>(base.Owner.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
			base.HandleOwnerEvent<BlockShieldEventArgs>(base.Owner.BlockShieldGaining, new GameEventHandler<BlockShieldEventArgs>(this.OnBlockShieldGaining));
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnded));
			base.React(this.FirstGain(base.Level));
		}

		// Token: 0x06000736 RID: 1846 RVA: 0x000157DB File Offset: 0x000139DB
		protected override void OnRemoving(Unit unit)
		{
			base.React(this.Lose(unit));
		}

		// Token: 0x06000737 RID: 1847 RVA: 0x000157EA File Offset: 0x000139EA
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				base.React(this.StackGain(other.Level));
			}
			return flag;
		}

		// Token: 0x06000738 RID: 1848 RVA: 0x00015808 File Offset: 0x00013A08
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

		// Token: 0x06000739 RID: 1849 RVA: 0x00015857 File Offset: 0x00013A57
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

		// Token: 0x0600073A RID: 1850 RVA: 0x00015891 File Offset: 0x00013A91
		private IEnumerable<BattleAction> OnTurnEnded(UnitEventArgs args)
		{
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}

		// Token: 0x0600073B RID: 1851 RVA: 0x000158A1 File Offset: 0x00013AA1
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

		// Token: 0x0600073C RID: 1852 RVA: 0x000158B8 File Offset: 0x00013AB8
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

		// Token: 0x0600073D RID: 1853 RVA: 0x000158CF File Offset: 0x00013ACF
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
