using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using UnityEngine;

namespace LBoL.Core.Units
{
	// Token: 0x0200008A RID: 138
	public abstract class Unit : GameEntity
	{
		// Token: 0x1700022F RID: 559
		// (get) Token: 0x060006BB RID: 1723 RVA: 0x000144B0 File Offset: 0x000126B0
		// (set) Token: 0x060006BC RID: 1724 RVA: 0x000144B8 File Offset: 0x000126B8
		public int TurnCounter { get; internal set; }

		// Token: 0x17000230 RID: 560
		// (get) Token: 0x060006BD RID: 1725 RVA: 0x000144C1 File Offset: 0x000126C1
		// (set) Token: 0x060006BE RID: 1726 RVA: 0x000144C9 File Offset: 0x000126C9
		public bool IsExtraTurn { get; internal set; }

		// Token: 0x17000231 RID: 561
		// (get) Token: 0x060006BF RID: 1727 RVA: 0x000144D2 File Offset: 0x000126D2
		// (set) Token: 0x060006C0 RID: 1728 RVA: 0x000144DA File Offset: 0x000126DA
		public bool IsInTurn { get; internal set; }

		// Token: 0x17000232 RID: 562
		// (get) Token: 0x060006C1 RID: 1729 RVA: 0x000144E3 File Offset: 0x000126E3
		// (set) Token: 0x060006C2 RID: 1730 RVA: 0x000144EB File Offset: 0x000126EB
		public int Hp { get; internal set; }

		// Token: 0x17000233 RID: 563
		// (get) Token: 0x060006C3 RID: 1731 RVA: 0x000144F4 File Offset: 0x000126F4
		// (set) Token: 0x060006C4 RID: 1732 RVA: 0x000144FC File Offset: 0x000126FC
		public int MaxHp { get; protected set; }

		// Token: 0x17000234 RID: 564
		// (get) Token: 0x060006C5 RID: 1733 RVA: 0x00014505 File Offset: 0x00012705
		// (set) Token: 0x060006C6 RID: 1734 RVA: 0x0001450D File Offset: 0x0001270D
		public int Shield { get; internal set; }

		// Token: 0x17000235 RID: 565
		// (get) Token: 0x060006C7 RID: 1735 RVA: 0x00014516 File Offset: 0x00012716
		// (set) Token: 0x060006C8 RID: 1736 RVA: 0x0001451E File Offset: 0x0001271E
		public int Block { get; internal set; }

		// Token: 0x17000236 RID: 566
		// (get) Token: 0x060006C9 RID: 1737 RVA: 0x00014527 File Offset: 0x00012727
		// (set) Token: 0x060006CA RID: 1738 RVA: 0x0001452F File Offset: 0x0001272F
		public UnitStatus Status { get; internal set; }

		// Token: 0x17000237 RID: 567
		// (get) Token: 0x060006CB RID: 1739 RVA: 0x00014538 File Offset: 0x00012738
		public bool IsAlive
		{
			get
			{
				return this.Status == UnitStatus.Alive;
			}
		}

		// Token: 0x17000238 RID: 568
		// (get) Token: 0x060006CC RID: 1740 RVA: 0x00014543 File Offset: 0x00012743
		public bool IsNotAlive
		{
			get
			{
				return this.Status > UnitStatus.Alive;
			}
		}

		// Token: 0x17000239 RID: 569
		// (get) Token: 0x060006CD RID: 1741 RVA: 0x0001454E File Offset: 0x0001274E
		public bool IsDying
		{
			get
			{
				return this.Status == UnitStatus.Dying;
			}
		}

		// Token: 0x1700023A RID: 570
		// (get) Token: 0x060006CE RID: 1742 RVA: 0x00014559 File Offset: 0x00012759
		public bool IsDead
		{
			get
			{
				return this.Status == UnitStatus.Dead;
			}
		}

		// Token: 0x1700023B RID: 571
		// (get) Token: 0x060006CF RID: 1743 RVA: 0x00014564 File Offset: 0x00012764
		public bool IsEscaped
		{
			get
			{
				return this.Status == UnitStatus.Escaped;
			}
		}

		// Token: 0x1700023C RID: 572
		// (get) Token: 0x060006D0 RID: 1744 RVA: 0x00014570 File Offset: 0x00012770
		public bool IsInvalidTarget
		{
			get
			{
				UnitStatus status = this.Status;
				return status == UnitStatus.Dead || status == UnitStatus.Escaped;
			}
		}

		// Token: 0x060006D1 RID: 1745 RVA: 0x00014595 File Offset: 0x00012795
		public override void Initialize()
		{
			base.Initialize();
			this.Hp = this.MaxHp;
		}

		// Token: 0x060006D2 RID: 1746
		public abstract UnitName GetName();

		// Token: 0x1700023D RID: 573
		// (get) Token: 0x060006D3 RID: 1747 RVA: 0x000145A9 File Offset: 0x000127A9
		public override string Name
		{
			get
			{
				return this.GetName().ToString();
			}
		}

		// Token: 0x1700023E RID: 574
		// (get) Token: 0x060006D4 RID: 1748 RVA: 0x000145B6 File Offset: 0x000127B6
		public string NameWithColor
		{
			get
			{
				return this.GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Default);
			}
		}

		// Token: 0x1700023F RID: 575
		// (get) Token: 0x060006D5 RID: 1749 RVA: 0x000145C6 File Offset: 0x000127C6
		public string ShortName
		{
			get
			{
				return this.GetName().ToString(UnitNameStyle.Short);
			}
		}

		// Token: 0x17000240 RID: 576
		// (get) Token: 0x060006D6 RID: 1750 RVA: 0x000145D4 File Offset: 0x000127D4
		public string ShortNameWithColor
		{
			get
			{
				return this.GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Short);
			}
		}

		// Token: 0x17000241 RID: 577
		// (get) Token: 0x060006D7 RID: 1751 RVA: 0x000145E4 File Offset: 0x000127E4
		public string FullName
		{
			get
			{
				return this.GetName().ToString(UnitNameStyle.Full);
			}
		}

		// Token: 0x17000242 RID: 578
		// (get) Token: 0x060006D8 RID: 1752 RVA: 0x000145F2 File Offset: 0x000127F2
		public string FullNameWithColor
		{
			get
			{
				return this.GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Full);
			}
		}

		// Token: 0x17000243 RID: 579
		// (get) Token: 0x060006D9 RID: 1753 RVA: 0x00014604 File Offset: 0x00012804
		// (set) Token: 0x060006DA RID: 1754 RVA: 0x00014623 File Offset: 0x00012823
		public BattleController Battle
		{
			get
			{
				BattleController battleController;
				if (!this._battle.TryGetTarget(ref battleController))
				{
					return null;
				}
				return battleController;
			}
			private set
			{
				this._battle.SetTarget(value);
			}
		}

		// Token: 0x060006DB RID: 1755 RVA: 0x00014631 File Offset: 0x00012831
		protected void HandleGameRunEvent<T>(GameEvent<T> e, GameEventHandler<T> handler, GameEventPriority priority) where T : GameEventArgs
		{
			this._gameRunHandlerHolder.HandleEvent<T>(e, handler, priority);
		}

		// Token: 0x060006DC RID: 1756 RVA: 0x00014641 File Offset: 0x00012841
		protected void HandleGameRunEvent<T>(GameEvent<T> e, GameEventHandler<T> handler) where T : GameEventArgs
		{
			this.HandleGameRunEvent<T>(e, handler, this.DefaultEventPriority);
		}

		// Token: 0x060006DD RID: 1757 RVA: 0x00014651 File Offset: 0x00012851
		protected void HandleBattleEvent<T>(GameEvent<T> e, GameEventHandler<T> handler, GameEventPriority priority) where T : GameEventArgs
		{
			this._battleHandlerHolder.HandleEvent<T>(e, handler, priority);
		}

		// Token: 0x060006DE RID: 1758 RVA: 0x00014661 File Offset: 0x00012861
		protected void HandleBattleEvent<T>(GameEvent<T> e, GameEventHandler<T> handler) where T : GameEventArgs
		{
			this.HandleBattleEvent<T>(e, handler, this.DefaultEventPriority);
		}

		// Token: 0x060006DF RID: 1759 RVA: 0x00014674 File Offset: 0x00012874
		protected void ReactBattleEvent<T>(GameEvent<T> e, Func<T, IEnumerable<BattleAction>> reactor, GameEventPriority priority) where T : GameEventArgs
		{
			this.HandleBattleEvent<T>(e, delegate(T args)
			{
				this.React(reactor.Invoke(args));
			}, priority);
		}

		// Token: 0x060006E0 RID: 1760 RVA: 0x000146A9 File Offset: 0x000128A9
		protected void ReactBattleEvent<T>(GameEvent<T> e, Func<T, IEnumerable<BattleAction>> reactor) where T : GameEventArgs
		{
			this.ReactBattleEvent<T>(e, reactor, this.DefaultEventPriority);
		}

		// Token: 0x060006E1 RID: 1761 RVA: 0x000146B9 File Offset: 0x000128B9
		internal virtual void EnterGameRun(GameRunController gameRun)
		{
			if (base.GameRun != null)
			{
				throw new InvalidOperationException("Cannot enter game-run while already in game-run");
			}
			base.GameRun = gameRun;
			this.OnEnterGameRun(gameRun);
		}

		// Token: 0x060006E2 RID: 1762 RVA: 0x000146DC File Offset: 0x000128DC
		internal void LeaveGameRun()
		{
			if (base.GameRun == null)
			{
				throw new InvalidOperationException("Cannot leave game-run while not in game-run");
			}
			this._gameRunHandlerHolder.ClearEventHandlers();
			this.OnLeaveGameRun();
			base.GameRun = null;
		}

		// Token: 0x060006E3 RID: 1763 RVA: 0x00014709 File Offset: 0x00012909
		protected virtual void OnEnterGameRun(GameRunController gameRun)
		{
		}

		// Token: 0x060006E4 RID: 1764 RVA: 0x0001470B File Offset: 0x0001290B
		protected virtual void OnLeaveGameRun()
		{
		}

		// Token: 0x060006E5 RID: 1765 RVA: 0x00014710 File Offset: 0x00012910
		internal void EnterBattle(BattleController battle)
		{
			if (base.GameRun != battle.GameRun)
			{
				throw new InvalidOperationException("Cannot enter battle while in different game-run");
			}
			if (this.Battle != null)
			{
				throw new InvalidOperationException("Cannot enter battle while already in battle");
			}
			this.Battle = battle;
			PlayerUnit playerUnit = this as PlayerUnit;
			if (playerUnit != null)
			{
				playerUnit.SetDollSlot(4);
			}
			this.OnEnterBattle(battle);
		}

		// Token: 0x060006E6 RID: 1766 RVA: 0x00014768 File Offset: 0x00012968
		internal void LeaveBattle()
		{
			if (this.Battle == null)
			{
				throw new InvalidOperationException("Cannot leave battle while not in battle");
			}
			this.TurnCounter = 0;
			this.ClearBlockShield();
			this.ClearStatusEffects();
			PlayerUnit playerUnit = this as PlayerUnit;
			if (playerUnit != null)
			{
				playerUnit.SetDollSlot(0);
			}
			this._battleHandlerHolder.ClearEventHandlers();
			this.OnLeaveBattle();
			this.Battle = null;
		}

		// Token: 0x060006E7 RID: 1767 RVA: 0x000147C4 File Offset: 0x000129C4
		protected virtual void OnEnterBattle(BattleController battle)
		{
		}

		// Token: 0x060006E8 RID: 1768 RVA: 0x000147C6 File Offset: 0x000129C6
		protected virtual void OnLeaveBattle()
		{
		}

		// Token: 0x060006E9 RID: 1769 RVA: 0x000147C8 File Offset: 0x000129C8
		protected override void React(Reactor reactor)
		{
			this.Battle.React(reactor, this, ActionCause.Unit);
		}

		// Token: 0x17000244 RID: 580
		// (get) Token: 0x060006EA RID: 1770 RVA: 0x000147D9 File Offset: 0x000129D9
		public IReadOnlyList<StatusEffect> StatusEffects
		{
			get
			{
				return this._statusEffects.AsReadOnly();
			}
		}

		// Token: 0x060006EB RID: 1771 RVA: 0x000147E8 File Offset: 0x000129E8
		public StatusEffect GetStatusEffect(Type type)
		{
			return Enumerable.FirstOrDefault<StatusEffect>(this._statusEffects, (StatusEffect effect) => effect.GetType() == type);
		}

		// Token: 0x060006EC RID: 1772 RVA: 0x00014819 File Offset: 0x00012A19
		public T GetStatusEffect<T>() where T : StatusEffect
		{
			return (T)((object)Enumerable.FirstOrDefault<StatusEffect>(this._statusEffects, (StatusEffect effect) => effect.GetType() == typeof(T)));
		}

		// Token: 0x060006ED RID: 1773 RVA: 0x0001484C File Offset: 0x00012A4C
		public StatusEffect GetStatusEffectExtend(Type type)
		{
			return Enumerable.FirstOrDefault<StatusEffect>(this._statusEffects, (StatusEffect effect) => effect.GetType().IsSubclassOf(type));
		}

		// Token: 0x060006EE RID: 1774 RVA: 0x0001487D File Offset: 0x00012A7D
		public T GetStatusEffectExtend<T>() where T : StatusEffect
		{
			return (T)((object)Enumerable.FirstOrDefault<StatusEffect>(this._statusEffects, (StatusEffect effect) => effect.GetType().IsSubclassOf(typeof(T))));
		}

		// Token: 0x060006EF RID: 1775 RVA: 0x000148B0 File Offset: 0x00012AB0
		public bool TryGetStatusEffect(Type type, out StatusEffect statusEffect)
		{
			statusEffect = Enumerable.FirstOrDefault<StatusEffect>(this._statusEffects, (StatusEffect effect) => effect.GetType() == type);
			return statusEffect != null;
		}

		// Token: 0x060006F0 RID: 1776 RVA: 0x000148E8 File Offset: 0x00012AE8
		public bool TryGetStatusEffect<T>(out T statusEffect) where T : StatusEffect
		{
			statusEffect = Enumerable.FirstOrDefault<StatusEffect>(this._statusEffects, (StatusEffect effect) => effect.GetType() == typeof(T)) as T;
			return statusEffect != null;
		}

		// Token: 0x060006F1 RID: 1777 RVA: 0x00014940 File Offset: 0x00012B40
		public bool HasStatusEffect(Type type)
		{
			return Enumerable.Any<StatusEffect>(this._statusEffects, (StatusEffect effect) => effect.GetType() == type);
		}

		// Token: 0x060006F2 RID: 1778 RVA: 0x00014971 File Offset: 0x00012B71
		public bool HasStatusEffect<T>() where T : StatusEffect
		{
			return Enumerable.Any<StatusEffect>(this._statusEffects, (StatusEffect effect) => effect.GetType() == typeof(T));
		}

		// Token: 0x060006F3 RID: 1779 RVA: 0x0001499D File Offset: 0x00012B9D
		public bool HasStatusEffect(StatusEffect effect)
		{
			return this._statusEffects.Contains(effect);
		}

		// Token: 0x060006F4 RID: 1780 RVA: 0x000149AC File Offset: 0x00012BAC
		internal StatusEffectAddResult? TryAddStatusEffect(StatusEffect effect)
		{
			if (effect.Owner != null)
			{
				Debug.LogError(string.Concat(new string[]
				{
					"Try to add effect ",
					effect.DebugName,
					" of ",
					effect.Owner.DebugName,
					" to ",
					this.DebugName
				}));
				StatusEffectAddResult? statusEffectAddResult = default(StatusEffectAddResult?);
				return statusEffectAddResult;
			}
			foreach (StatusEffect statusEffect in this._statusEffects)
			{
				OpposeResult? opposeResult = statusEffect.Oppose(effect);
				OpposeResult? opposeResult2 = opposeResult;
				OpposeResult opposeResult3 = OpposeResult.KeepSelf;
				if ((opposeResult2.GetValueOrDefault() == opposeResult3) & (opposeResult2 != null))
				{
					return new StatusEffectAddResult?(StatusEffectAddResult.Opposed);
				}
				opposeResult2 = opposeResult;
				opposeResult3 = OpposeResult.Neutralize;
				if ((opposeResult2.GetValueOrDefault() == opposeResult3) & (opposeResult2 != null))
				{
					this.React(new RemoveStatusEffectAction(statusEffect, true, 0.1f));
					return new StatusEffectAddResult?(StatusEffectAddResult.Neutralized);
				}
				opposeResult2 = opposeResult;
				opposeResult3 = OpposeResult.KeepOther;
				if ((opposeResult2.GetValueOrDefault() == opposeResult3) & (opposeResult2 != null))
				{
					this.React(new RemoveStatusEffectAction(statusEffect, true, 0.1f));
				}
			}
			StatusEffect statusEffect2 = this.GetStatusEffect(effect.GetType());
			if (statusEffect2 != null && statusEffect2.Stack(effect))
			{
				return new StatusEffectAddResult?(StatusEffectAddResult.Stacked);
			}
			int? stackActionTriggerLevel = effect.Config.StackActionTriggerLevel;
			if (stackActionTriggerLevel != null)
			{
				int valueOrDefault = stackActionTriggerLevel.GetValueOrDefault();
				if (effect.Level >= valueOrDefault)
				{
					int num = effect.Level / valueOrDefault;
					effect.Level %= valueOrDefault;
					this.Battle.React(new Reactor(effect.StackAction(this, num)), effect, ActionCause.StatusEffect);
				}
				if (effect.Level == 0)
				{
					return new StatusEffectAddResult?(StatusEffectAddResult.Stacked);
				}
			}
			effect.Owner = this;
			effect.TriggerAdding(this);
			effect.ClampMax();
			this._statusEffects.Add(effect);
			effect.TriggerAdded(this);
			if (effect.Type == StatusEffectType.Positive && this is PlayerUnit && base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>())
			{
				if (Enumerable.Count<StatusEffect>(this._statusEffects, (StatusEffect e) => e.Type == StatusEffectType.Positive) >= 20)
				{
					base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.JiangshenYishi);
				}
			}
			return new StatusEffectAddResult?(StatusEffectAddResult.Added);
		}

		// Token: 0x060006F5 RID: 1781 RVA: 0x00014C20 File Offset: 0x00012E20
		internal bool TryRemoveStatusEffect(StatusEffect effect)
		{
			if (!this._statusEffects.Contains(effect))
			{
				return false;
			}
			effect.TriggerRemoving(this);
			this._statusEffects.Remove(effect);
			effect.TriggerRemoved(this);
			effect.Owner = null;
			return true;
		}

		// Token: 0x060006F6 RID: 1782 RVA: 0x00014C55 File Offset: 0x00012E55
		private void ClearBlockShield()
		{
			this.Block = 0;
			this.Shield = 0;
		}

		// Token: 0x060006F7 RID: 1783 RVA: 0x00014C68 File Offset: 0x00012E68
		private void ClearStatusEffects()
		{
			foreach (StatusEffect statusEffect in Enumerable.ToList<StatusEffect>(this._statusEffects))
			{
				if (!this.TryRemoveStatusEffect(statusEffect))
				{
					Debug.LogError("Cannot clear " + statusEffect.DebugName + " on " + this.DebugName);
				}
			}
			if (Enumerable.Any<StatusEffect>(this._statusEffects))
			{
				string[] array = new string[5];
				array[0] = "Clearing ";
				array[1] = this.DebugName;
				array[2] = "'s effect incomplete: [";
				array[3] = string.Join(", ", Enumerable.Select<StatusEffect, string>(this._statusEffects, (StatusEffect e) => e.DebugName));
				array[4] = "]";
				Debug.LogError(string.Concat(array));
			}
		}

		// Token: 0x17000245 RID: 581
		// (get) Token: 0x060006F8 RID: 1784 RVA: 0x00014D58 File Offset: 0x00012F58
		public int TotalFirepower
		{
			get
			{
				TempFirepower statusEffect = this.GetStatusEffect<TempFirepower>();
				int num = ((statusEffect != null) ? statusEffect.Level : 0);
				Firepower statusEffect2 = this.GetStatusEffect<Firepower>();
				int num2 = num + ((statusEffect2 != null) ? statusEffect2.Level : 0);
				TempFirepowerNegative statusEffect3 = this.GetStatusEffect<TempFirepowerNegative>();
				int num3 = num2 - ((statusEffect3 != null) ? statusEffect3.Level : 0);
				FirepowerNegative statusEffect4 = this.GetStatusEffect<FirepowerNegative>();
				int num4 = num3 - ((statusEffect4 != null) ? statusEffect4.Level : 0);
				Grace statusEffect5 = this.GetStatusEffect<Grace>();
				return num4 + ((statusEffect5 != null) ? statusEffect5.Level : 0);
			}
		}

		// Token: 0x17000246 RID: 582
		// (get) Token: 0x060006F9 RID: 1785 RVA: 0x00014DC4 File Offset: 0x00012FC4
		public int TotalSpirit
		{
			get
			{
				TempSpirit statusEffect = this.GetStatusEffect<TempSpirit>();
				int num = ((statusEffect != null) ? statusEffect.Level : 0);
				Spirit statusEffect2 = this.GetStatusEffect<Spirit>();
				int num2 = num + ((statusEffect2 != null) ? statusEffect2.Level : 0);
				TempSpiritNegative statusEffect3 = this.GetStatusEffect<TempSpiritNegative>();
				int num3 = num2 - ((statusEffect3 != null) ? statusEffect3.Level : 0);
				SpiritNegative statusEffect4 = this.GetStatusEffect<SpiritNegative>();
				int num4 = num3 - ((statusEffect4 != null) ? statusEffect4.Level : 0);
				Grace statusEffect5 = this.GetStatusEffect<Grace>();
				return num4 + ((statusEffect5 != null) ? statusEffect5.Level : 0);
			}
		}

		// Token: 0x060006FA RID: 1786 RVA: 0x00014E30 File Offset: 0x00013030
		internal DamageInfo MeasureDamage(DamageInfo info)
		{
			if (info.Damage < 0f)
			{
				info.Damage = 0f;
			}
			else
			{
				info.Damage = info.Damage.Round(1);
			}
			if (info.DamageType == DamageType.Attack && info.Damage > 0f && this.HasStatusEffect<Graze>() && !info.IsAccuracy)
			{
				return new DamageInfo(0f, info.DamageType, true, false, false).BlockBy(this.Block).ShieldBy(this.Shield);
			}
			if (info.DamageType != DamageType.HpLose)
			{
				return info.BlockBy(this.Block).ShieldBy(this.Shield);
			}
			return info;
		}

		// Token: 0x060006FB RID: 1787 RVA: 0x00014EF0 File Offset: 0x000130F0
		internal DamageInfo TakeDamage(DamageInfo info)
		{
			if (info.IsGrazed && info.IsAccuracy)
			{
				Debug.LogError(string.Format("Taking grazed accurary DamageInfo {0}.", info));
			}
			if (this.IsDead)
			{
				Debug.LogError("Damaging already dead " + this.DebugName);
			}
			if (this.IsDying)
			{
				Debug.LogWarning("Damaging dying unit " + this.DebugName);
			}
			if (info.DamageType == DamageType.Attack)
			{
				if (info.IsGrazed)
				{
					this.GetStatusEffect<Graze>().Activate();
				}
				else if (info.IsAccuracy && this.HasStatusEffect<Graze>())
				{
					this.GetStatusEffect<Graze>().BeenAccurate();
				}
			}
			int num = Unit.<TakeDamage>g__CheckIsInt|100_0(info.Damage, "Damage is not int", "Damage is negative");
			int num2 = Unit.<TakeDamage>g__CheckIsInt|100_0(info.DamageBlocked, "Damage blocked is not int", "Damage blocked is negative");
			int num3 = Unit.<TakeDamage>g__CheckIsInt|100_0(info.DamageShielded, "Damage shielded is not int", "Damage shielded is negative");
			this.Hp -= num;
			if (this.Hp < 0)
			{
				info.OverDamage = -this.Hp;
				this.Hp = 0;
			}
			if (this.Block < num2)
			{
				Debug.LogError(string.Format("{0} taking damage is over-blocked (block={1}, taking={2})", this.DebugName, this.Block, num2));
				this.Block = 0;
			}
			else
			{
				this.Block -= num2;
			}
			if (this.Shield < num3)
			{
				Debug.LogError(string.Format("{0} taking damage is over-shielded (shield={1}, taking={2}", this.DebugName, this.Shield, num3));
				this.Shield = 0;
			}
			else
			{
				this.Shield -= num3;
			}
			return info;
		}

		// Token: 0x060006FC RID: 1788 RVA: 0x00015098 File Offset: 0x00013298
		internal void SetMaxHp(int hp, int maxHp)
		{
			if (hp < 0)
			{
				throw new ArgumentException("Hp cannot be negative", "hp");
			}
			if (maxHp < 0)
			{
				throw new ArgumentException("Max hp cannot be negative", "maxHp");
			}
			this.MaxHp = Math.Min(maxHp, 9999);
			this.Hp = Math.Min(hp, this.MaxHp);
		}

		// Token: 0x060006FD RID: 1789 RVA: 0x000150F0 File Offset: 0x000132F0
		internal int Heal(int healValue)
		{
			int num = Math.Min(this.MaxHp - this.Hp, healValue);
			this.Hp += num;
			return num;
		}

		// Token: 0x060006FE RID: 1790 RVA: 0x00015120 File Offset: 0x00013320
		[return: TupleElementNames(new string[] { "block", "shield" })]
		internal ValueTuple<int, int> GainBlockShield(float block, float shield)
		{
			int num = Math.Min(block.RoundToInt(1), 9999 - this.Block);
			int num2 = Math.Min(shield.RoundToInt(1), 9999 - this.Shield);
			this.Block += num;
			this.Shield += num2;
			if (this.Shield >= 1000 && this is PlayerUnit && base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>())
			{
				base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.BoliJiejie);
			}
			return new ValueTuple<int, int>(num, num2);
		}

		// Token: 0x060006FF RID: 1791 RVA: 0x000151C8 File Offset: 0x000133C8
		[return: TupleElementNames(new string[] { "block", "shield" })]
		internal ValueTuple<int, int> LoseBlockShield(float block, float shield)
		{
			int num = Math.Min(this.Block, block.RoundToInt(1));
			int num2 = Math.Min(this.Shield, shield.RoundToInt(1));
			this.Block -= num;
			this.Shield -= num2;
			return new ValueTuple<int, int>(num, num2);
		}

		// Token: 0x06000700 RID: 1792 RVA: 0x00015220 File Offset: 0x00013420
		internal virtual void Die()
		{
			this.OnDie();
			UnitStatus status = this.Status;
			if (status == UnitStatus.Alive || status == UnitStatus.Dead)
			{
				Debug.LogError(string.Format("Unit die when {0}", this.Status));
			}
			this.Status = UnitStatus.Dead;
		}

		// Token: 0x06000701 RID: 1793 RVA: 0x00015262 File Offset: 0x00013462
		protected virtual void OnDie()
		{
		}

		// Token: 0x06000702 RID: 1794 RVA: 0x00015264 File Offset: 0x00013464
		public virtual void SetView(IUnitView view)
		{
			this.View = view;
		}

		// Token: 0x06000703 RID: 1795 RVA: 0x00015270 File Offset: 0x00013470
		public virtual TView GetView<TView>() where TView : class, IUnitView
		{
			TView tview = this.View as TView;
			if (tview == null)
			{
				return default(TView);
			}
			return tview;
		}

		// Token: 0x17000247 RID: 583
		// (get) Token: 0x06000704 RID: 1796 RVA: 0x000152A1 File Offset: 0x000134A1
		// (set) Token: 0x06000705 RID: 1797 RVA: 0x000152A9 File Offset: 0x000134A9
		public IUnitView View { get; private set; }

		// Token: 0x17000248 RID: 584
		// (get) Token: 0x06000706 RID: 1798 RVA: 0x000152B2 File Offset: 0x000134B2
		public GameEvent<UnitEventArgs> TurnStarting { get; } = new GameEvent<UnitEventArgs>();

		// Token: 0x17000249 RID: 585
		// (get) Token: 0x06000707 RID: 1799 RVA: 0x000152BA File Offset: 0x000134BA
		public GameEvent<UnitEventArgs> TurnStarted { get; } = new GameEvent<UnitEventArgs>();

		// Token: 0x1700024A RID: 586
		// (get) Token: 0x06000708 RID: 1800 RVA: 0x000152C2 File Offset: 0x000134C2
		public GameEvent<UnitEventArgs> TurnEnding { get; } = new GameEvent<UnitEventArgs>();

		// Token: 0x1700024B RID: 587
		// (get) Token: 0x06000709 RID: 1801 RVA: 0x000152CA File Offset: 0x000134CA
		public GameEvent<UnitEventArgs> TurnEnded { get; } = new GameEvent<UnitEventArgs>();

		// Token: 0x1700024C RID: 588
		// (get) Token: 0x0600070A RID: 1802 RVA: 0x000152D2 File Offset: 0x000134D2
		public GameEvent<BlockShieldEventArgs> BlockShieldCasting { get; } = new GameEvent<BlockShieldEventArgs>();

		// Token: 0x1700024D RID: 589
		// (get) Token: 0x0600070B RID: 1803 RVA: 0x000152DA File Offset: 0x000134DA
		public GameEvent<BlockShieldEventArgs> BlockShieldGaining { get; } = new GameEvent<BlockShieldEventArgs>();

		// Token: 0x1700024E RID: 590
		// (get) Token: 0x0600070C RID: 1804 RVA: 0x000152E2 File Offset: 0x000134E2
		public GameEvent<BlockShieldEventArgs> BlockShieldCasted { get; } = new GameEvent<BlockShieldEventArgs>();

		// Token: 0x1700024F RID: 591
		// (get) Token: 0x0600070D RID: 1805 RVA: 0x000152EA File Offset: 0x000134EA
		public GameEvent<BlockShieldEventArgs> BlockShieldGained { get; } = new GameEvent<BlockShieldEventArgs>();

		// Token: 0x17000250 RID: 592
		// (get) Token: 0x0600070E RID: 1806 RVA: 0x000152F2 File Offset: 0x000134F2
		public GameEvent<BlockShieldEventArgs> BlockShieldLosing { get; } = new GameEvent<BlockShieldEventArgs>();

		// Token: 0x17000251 RID: 593
		// (get) Token: 0x0600070F RID: 1807 RVA: 0x000152FA File Offset: 0x000134FA
		public GameEvent<BlockShieldEventArgs> BlockShieldLost { get; } = new GameEvent<BlockShieldEventArgs>();

		// Token: 0x17000252 RID: 594
		// (get) Token: 0x06000710 RID: 1808 RVA: 0x00015302 File Offset: 0x00013502
		public GameEvent<DamageDealingEventArgs> DamageDealing { get; } = new GameEvent<DamageDealingEventArgs>();

		// Token: 0x17000253 RID: 595
		// (get) Token: 0x06000711 RID: 1809 RVA: 0x0001530A File Offset: 0x0001350A
		public GameEvent<DamageEventArgs> DamageReceiving { get; } = new GameEvent<DamageEventArgs>();

		// Token: 0x17000254 RID: 596
		// (get) Token: 0x06000712 RID: 1810 RVA: 0x00015312 File Offset: 0x00013512
		public GameEvent<DamageEventArgs> DamageGiving { get; } = new GameEvent<DamageEventArgs>();

		// Token: 0x17000255 RID: 597
		// (get) Token: 0x06000713 RID: 1811 RVA: 0x0001531A File Offset: 0x0001351A
		public GameEvent<DamageEventArgs> DamageTaking { get; } = new GameEvent<DamageEventArgs>();

		// Token: 0x17000256 RID: 598
		// (get) Token: 0x06000714 RID: 1812 RVA: 0x00015322 File Offset: 0x00013522
		public GameEvent<DamageEventArgs> DamageDealt { get; } = new GameEvent<DamageEventArgs>();

		// Token: 0x17000257 RID: 599
		// (get) Token: 0x06000715 RID: 1813 RVA: 0x0001532A File Offset: 0x0001352A
		public GameEvent<DamageEventArgs> DamageReceived { get; } = new GameEvent<DamageEventArgs>();

		// Token: 0x17000258 RID: 600
		// (get) Token: 0x06000716 RID: 1814 RVA: 0x00015332 File Offset: 0x00013532
		public GameEvent<HealEventArgs> HealingGiving { get; } = new GameEvent<HealEventArgs>();

		// Token: 0x17000259 RID: 601
		// (get) Token: 0x06000717 RID: 1815 RVA: 0x0001533A File Offset: 0x0001353A
		public GameEvent<HealEventArgs> HealingReceiving { get; } = new GameEvent<HealEventArgs>();

		// Token: 0x1700025A RID: 602
		// (get) Token: 0x06000718 RID: 1816 RVA: 0x00015342 File Offset: 0x00013542
		public GameEvent<HealEventArgs> HealingGiven { get; } = new GameEvent<HealEventArgs>();

		// Token: 0x1700025B RID: 603
		// (get) Token: 0x06000719 RID: 1817 RVA: 0x0001534A File Offset: 0x0001354A
		public GameEvent<HealEventArgs> HealingReceived { get; } = new GameEvent<HealEventArgs>();

		// Token: 0x1700025C RID: 604
		// (get) Token: 0x0600071A RID: 1818 RVA: 0x00015352 File Offset: 0x00013552
		public GameEvent<DollValueArgs> DollValueGenerating { get; } = new GameEvent<DollValueArgs>();

		// Token: 0x1700025D RID: 605
		// (get) Token: 0x0600071B RID: 1819 RVA: 0x0001535A File Offset: 0x0001355A
		public GameEvent<StatusEffectApplyEventArgs> StatusEffectAdding { get; } = new GameEvent<StatusEffectApplyEventArgs>();

		// Token: 0x1700025E RID: 606
		// (get) Token: 0x0600071C RID: 1820 RVA: 0x00015362 File Offset: 0x00013562
		public GameEvent<StatusEffectApplyEventArgs> StatusEffectAdded { get; } = new GameEvent<StatusEffectApplyEventArgs>();

		// Token: 0x1700025F RID: 607
		// (get) Token: 0x0600071D RID: 1821 RVA: 0x0001536A File Offset: 0x0001356A
		public GameEvent<StatusEffectEventArgs> StatusEffectRemoving { get; } = new GameEvent<StatusEffectEventArgs>();

		// Token: 0x17000260 RID: 608
		// (get) Token: 0x0600071E RID: 1822 RVA: 0x00015372 File Offset: 0x00013572
		public GameEvent<StatusEffectEventArgs> StatusEffectRemoved { get; } = new GameEvent<StatusEffectEventArgs>();

		// Token: 0x17000261 RID: 609
		// (get) Token: 0x0600071F RID: 1823 RVA: 0x0001537A File Offset: 0x0001357A
		public GameEvent<StatusEffectEventArgs> StatusEffectChanged { get; } = new GameEvent<StatusEffectEventArgs>();

		// Token: 0x17000262 RID: 610
		// (get) Token: 0x06000720 RID: 1824 RVA: 0x00015382 File Offset: 0x00013582
		public GameEvent<MoodChangeEventArgs> MoodChanging { get; } = new GameEvent<MoodChangeEventArgs>();

		// Token: 0x17000263 RID: 611
		// (get) Token: 0x06000721 RID: 1825 RVA: 0x0001538A File Offset: 0x0001358A
		public GameEvent<MoodChangeEventArgs> MoodChanged { get; } = new GameEvent<MoodChangeEventArgs>();

		// Token: 0x17000264 RID: 612
		// (get) Token: 0x06000722 RID: 1826 RVA: 0x00015392 File Offset: 0x00013592
		public GameEvent<DieEventArgs> Dying { get; } = new GameEvent<DieEventArgs>();

		// Token: 0x17000265 RID: 613
		// (get) Token: 0x06000723 RID: 1827 RVA: 0x0001539A File Offset: 0x0001359A
		public GameEvent<DieEventArgs> Died { get; } = new GameEvent<DieEventArgs>();

		// Token: 0x17000266 RID: 614
		// (get) Token: 0x06000724 RID: 1828 RVA: 0x000153A2 File Offset: 0x000135A2
		public GameEvent<UnitEventArgs> Escaping { get; } = new GameEvent<UnitEventArgs>();

		// Token: 0x17000267 RID: 615
		// (get) Token: 0x06000725 RID: 1829 RVA: 0x000153AA File Offset: 0x000135AA
		public GameEvent<UnitEventArgs> Escaped { get; } = new GameEvent<UnitEventArgs>();

		// Token: 0x17000268 RID: 616
		// (get) Token: 0x06000726 RID: 1830 RVA: 0x000153B2 File Offset: 0x000135B2
		public GameEvent<StatisticalDamageEventArgs> StatisticalTotalDamageDealt { get; } = new GameEvent<StatisticalDamageEventArgs>();

		// Token: 0x17000269 RID: 617
		// (get) Token: 0x06000727 RID: 1831 RVA: 0x000153BA File Offset: 0x000135BA
		public GameEvent<StatisticalDamageEventArgs> StatisticalTotalDamageReceived { get; } = new GameEvent<StatisticalDamageEventArgs>();

		// Token: 0x06000729 RID: 1833 RVA: 0x0001559C File Offset: 0x0001379C
		[CompilerGenerated]
		internal static int <TakeDamage>g__CheckIsInt|100_0(float value, string roundError, string negativeError)
		{
			int num = (int)value;
			if (!value.Approximately((float)num))
			{
				Debug.LogError(string.Concat(new string[]
				{
					roundError,
					" (",
					num.ToString(),
					" vs ",
					value.ToString(),
					")"
				}));
				return value.RoundToInt(1);
			}
			return num;
		}

		// Token: 0x04000321 RID: 801
		private readonly WeakReference<BattleController> _battle = new WeakReference<BattleController>(null);

		// Token: 0x04000322 RID: 802
		private readonly GameEventHandlerHolder _battleHandlerHolder = new GameEventHandlerHolder();

		// Token: 0x04000323 RID: 803
		private readonly GameEventHandlerHolder _gameRunHandlerHolder = new GameEventHandlerHolder();

		// Token: 0x04000324 RID: 804
		private readonly OrderedList<StatusEffect> _statusEffects = new OrderedList<StatusEffect>((StatusEffect a, StatusEffect b) => a.Config.Order.CompareTo(b.Config.Order));
	}
}
