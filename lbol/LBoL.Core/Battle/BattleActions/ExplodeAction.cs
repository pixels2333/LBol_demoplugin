using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000180 RID: 384
	public sealed class ExplodeAction : BattleAction
	{
		// Token: 0x17000506 RID: 1286
		// (get) Token: 0x06000E81 RID: 3713 RVA: 0x00027722 File Offset: 0x00025922
		public DieEventArgs DieArgs { get; }

		// Token: 0x17000507 RID: 1287
		// (get) Token: 0x06000E82 RID: 3714 RVA: 0x0002772A File Offset: 0x0002592A
		public DamageDealingEventArgs DealingArgs { get; }

		// Token: 0x17000508 RID: 1288
		// (get) Token: 0x06000E83 RID: 3715 RVA: 0x00027732 File Offset: 0x00025932
		public DamageEventArgs[] DamageArgs { get; }

		// Token: 0x17000509 RID: 1289
		// (get) Token: 0x06000E84 RID: 3716 RVA: 0x0002773A File Offset: 0x0002593A
		public string GunName { get; }

		// Token: 0x1700050A RID: 1290
		// (get) Token: 0x06000E85 RID: 3717 RVA: 0x00027742 File Offset: 0x00025942
		public GunType GunType { get; }

		// Token: 0x06000E86 RID: 3718 RVA: 0x0002774C File Offset: 0x0002594C
		public ExplodeAction(EnemyUnit unit, IEnumerable<Unit> damageTargets, DamageInfo damageInfo, DieCause dieCause, GameEntity dieSource, string gunName = "Instant", GunType gunType = GunType.Single)
		{
			Unit[] array = Enumerable.ToArray<Unit>(damageTargets);
			this.DealingArgs = new DamageDealingEventArgs
			{
				Source = unit,
				Targets = array,
				GunName = gunName,
				DamageInfo = damageInfo
			};
			this.DamageArgs = Enumerable.ToArray<DamageEventArgs>(Enumerable.Select<Unit, DamageEventArgs>(array, (Unit t) => new DamageEventArgs
			{
				Source = unit,
				Target = t,
				GunName = gunName
			}));
			this.DieArgs = new DieEventArgs
			{
				Unit = unit,
				Source = unit,
				DieCause = dieCause,
				DieSource = dieSource,
				CanCancel = false
			};
			this.GunName = gunName;
			this.GunType = gunType;
		}

		// Token: 0x06000E87 RID: 3719 RVA: 0x00027817 File Offset: 0x00025A17
		public ExplodeAction(EnemyUnit unit, Unit damageTarget, DamageInfo damageInfo, DieCause dieCause, GameEntity dieSource, string gunName = "Instant", GunType gunType = GunType.Single)
			: this(unit, new Unit[] { damageTarget }, damageInfo, dieCause, dieSource, gunName, gunType)
		{
		}

		// Token: 0x06000E88 RID: 3720 RVA: 0x00027834 File Offset: 0x00025A34
		public override BattleAction SetCause(ActionCause cause)
		{
			base.SetCause(cause);
			this.DieArgs.Cause = cause;
			this.DealingArgs.Cause = cause;
			DamageEventArgs[] damageArgs = this.DamageArgs;
			for (int i = 0; i < damageArgs.Length; i++)
			{
				damageArgs[i].Cause = cause;
			}
			return this;
		}

		// Token: 0x06000E89 RID: 3721 RVA: 0x00027880 File Offset: 0x00025A80
		public override BattleAction SetSource(GameEntity source)
		{
			base.SetSource(source);
			this.DieArgs.ActionSource = source;
			this.DealingArgs.ActionSource = source;
			DamageEventArgs[] damageArgs = this.DamageArgs;
			for (int i = 0; i < damageArgs.Length; i++)
			{
				damageArgs[i].ActionSource = source;
			}
			return this;
		}

		// Token: 0x1700050B RID: 1291
		// (get) Token: 0x06000E8A RID: 3722 RVA: 0x000278CC File Offset: 0x00025ACC
		public override bool IsModified
		{
			get
			{
				if (!this._dealingPassed)
				{
					return this.DealingArgs.IsModified;
				}
				if (!this.DieArgs.IsModified)
				{
					return Enumerable.Any<DamageEventArgs>(this.DamageArgs, (DamageEventArgs a) => a.IsModified);
				}
				return true;
			}
		}

		// Token: 0x1700050C RID: 1292
		// (get) Token: 0x06000E8B RID: 3723 RVA: 0x00027928 File Offset: 0x00025B28
		public override string[] Modifiers
		{
			get
			{
				if (!this._dealingPassed)
				{
					return this.DealingArgs.Modifiers;
				}
				return Enumerable.ToArray<string>(Enumerable.Distinct<string>(Enumerable.Concat<string>(this.DieArgs.Modifiers, Enumerable.SelectMany<DamageEventArgs, string>(this.DamageArgs, (DamageEventArgs arg) => arg.Modifiers))));
			}
		}

		// Token: 0x1700050D RID: 1293
		// (get) Token: 0x06000E8C RID: 3724 RVA: 0x0002798D File Offset: 0x00025B8D
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700050E RID: 1294
		// (get) Token: 0x06000E8D RID: 3725 RVA: 0x00027990 File Offset: 0x00025B90
		public override CancelCause CancelCause
		{
			get
			{
				return CancelCause.None;
			}
		}

		// Token: 0x06000E8E RID: 3726 RVA: 0x00027994 File Offset: 0x00025B94
		public override void ClearModifiers()
		{
			this.DieArgs.ClearModifiers();
			DamageEventArgs[] damageArgs = this.DamageArgs;
			for (int i = 0; i < damageArgs.Length; i++)
			{
				damageArgs[i].ClearModifiers();
			}
		}

		// Token: 0x06000E8F RID: 3727 RVA: 0x000279C9 File Offset: 0x00025BC9
		internal override IEnumerable<Phase> GetPhases()
		{
			if (Enumerable.All<Unit>(this.DealingArgs.Targets, (Unit t) => t.IsDead))
			{
				Debug.LogWarning("[ExplodeAction] Targets [" + string.Join(", ", Enumerable.Select<Unit, string>(this.DealingArgs.Targets, (Unit t) => t.Name)) + "] all dead, action is canceled");
				yield break;
			}
			Unit source = this.DealingArgs.Source;
			if (source != null)
			{
				yield return base.CreateEventPhase<DamageDealingEventArgs>("DamageDealing", this.DealingArgs, source.DamageDealing);
			}
			this._dealingPassed = true;
			DamageInfo damageInfo = this.DealingArgs.DamageInfo;
			foreach (ValueTuple<Unit, DamageEventArgs> valueTuple in this.DealingArgs.Targets.Zip(this.DamageArgs))
			{
				Unit item = valueTuple.Item1;
				DamageEventArgs item2 = valueTuple.Item2;
				item2.DamageInfo = damageInfo;
				if (item2.Target.IsInvalidTarget)
				{
					item2.ForceCancelBecause(CancelCause.InvalidTarget);
				}
				else
				{
					yield return base.CreateEventPhase<DamageEventArgs>("DamageReceiving", item2, item.DamageReceiving);
				}
			}
			IEnumerator<ValueTuple<Unit, DamageEventArgs>> enumerator = null;
			yield return base.CreatePhase("MeasureDamage", delegate
			{
				foreach (DamageEventArgs damageEventArgs6 in this.DamageArgs)
				{
					if (damageEventArgs6.IsCanceled)
					{
						return;
					}
					damageEventArgs6.DamageInfo = damageEventArgs6.Target.MeasureDamage(damageEventArgs6.DamageInfo);
					damageEventArgs6.IsModified = true;
				}
			}, false);
			foreach (DamageEventArgs damageEventArgs in this.DamageArgs)
			{
				Unit source2 = damageEventArgs.Source;
				if (source2 != null && !damageEventArgs.IsCanceled)
				{
					yield return base.CreateEventPhase<DamageEventArgs>("DamageGiving", damageEventArgs, source2.DamageGiving);
				}
			}
			DamageEventArgs[] array = null;
			foreach (DamageEventArgs damageEventArgs2 in this.DamageArgs)
			{
				if (!damageEventArgs2.IsCanceled)
				{
					yield return base.CreateEventPhase<DamageEventArgs>("DamageTaking", damageEventArgs2, damageEventArgs2.Target.DamageTaking);
				}
			}
			array = null;
			yield return base.CreatePhase("Die", delegate
			{
				this.DieArgs.Unit.Status = UnitStatus.Dying;
				EnemyUnit enemyUnit = (EnemyUnit)this.DieArgs.Unit;
				DieEventArgs dieArgs = this.DieArgs;
				DieEventArgs dieArgs2 = this.DieArgs;
				DieEventArgs dieArgs3 = this.DieArgs;
				ValueTuple<int, int, int> valueTuple2 = this.Battle.GenerateEnemyPoints(enemyUnit);
				dieArgs.Power = valueTuple2.Item1;
				dieArgs2.BluePoint = valueTuple2.Item2;
				dieArgs3.Money = valueTuple2.Item3;
				enemyUnit.EnemyPointGenerating.Execute(this.DieArgs);
				this.Battle.EnemyPointGenerating.Execute(this.DieArgs);
				this.Battle.Die(this.DieArgs.Unit, this.DieArgs.Source == this.DieArgs.Unit, this.DieArgs.Power, this.DieArgs.BluePoint, this.DieArgs.Money);
				this.DieArgs.IsModified = true;
				this.DieArgs.Unit.Died.Execute(this.DieArgs);
				this.Battle.EnemyDied.Execute(this.DieArgs);
			}, false);
			yield return base.CreatePhase("Main", delegate
			{
				foreach (DamageEventArgs damageEventArgs7 in this.DamageArgs)
				{
					if (damageEventArgs7.IsCanceled)
					{
						return;
					}
					damageEventArgs7.DamageInfo = this.Battle.Damage(damageEventArgs7.Source, damageEventArgs7.Target, damageEventArgs7.DamageInfo, this.Source);
					damageEventArgs7.IsModified = true;
				}
			}, true);
			List<ValueTuple<Unit, DieCause>> deadUnits = new List<ValueTuple<Unit, DieCause>>();
			foreach (DamageEventArgs damageEventArgs3 in this.DamageArgs)
			{
				Unit target = damageEventArgs3.Target;
				if (target.Hp == 0 && target.Status == UnitStatus.Alive)
				{
					target.Status = UnitStatus.Dying;
					DieCause dieCause;
					switch (damageEventArgs3.DamageInfo.DamageType)
					{
					case DamageType.HpLose:
						dieCause = DieCause.LoseHp;
						break;
					case DamageType.Reaction:
						dieCause = DieCause.Reaction;
						break;
					case DamageType.Attack:
						dieCause = DieCause.Attack;
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
					DieCause dieCause2 = dieCause;
					deadUnits.Add(new ValueTuple<Unit, DieCause>(target, dieCause2));
				}
			}
			if (deadUnits.Count > 0)
			{
				yield return base.CreatePhase("Dying", delegate
				{
					this.React(new DieAction(deadUnits, this.DealingArgs.Source, this.DealingArgs.ActionSource), this.Source, default(ActionCause?));
				}, false);
			}
			foreach (DamageEventArgs damageEventArgs4 in this.DamageArgs)
			{
				if (!damageEventArgs4.IsCanceled)
				{
					Unit source3 = damageEventArgs4.Source;
					if (source3 != null)
					{
						yield return base.CreateEventPhase<DamageEventArgs>("DamageDealt", damageEventArgs4, source3.DamageDealt);
					}
				}
			}
			array = null;
			foreach (DamageEventArgs damageEventArgs5 in this.DamageArgs)
			{
				if (!damageEventArgs5.IsCanceled)
				{
					yield return base.CreateEventPhase<DamageEventArgs>("DamageReceived", damageEventArgs5, damageEventArgs5.Target.DamageReceived);
				}
			}
			array = null;
			yield return base.CreatePhase("ClearEffects", delegate
			{
				foreach (StatusEffect statusEffect in Enumerable.ToList<StatusEffect>(this.DieArgs.Unit.StatusEffects))
				{
					this.Battle.React(new RemoveStatusEffectAction(statusEffect, true, 0.1f), null, ActionCause.None);
				}
			}, false);
			yield break;
			yield break;
		}

		// Token: 0x06000E90 RID: 3728 RVA: 0x000279DC File Offset: 0x00025BDC
		public override string ExportDebugDetails()
		{
			if (!this._dealingPassed)
			{
				return "Damage: " + this.DealingArgs.ExportDebugDetails() + "; Die: " + this.DieArgs.ExportDebugDetails();
			}
			return "Damage: [" + string.Join(", ", Enumerable.Select<DamageEventArgs, string>(this.DamageArgs, (DamageEventArgs a) => a.ExportDebugDetails())) + "]; Die: " + this.DieArgs.ExportDebugDetails();
		}

		// Token: 0x0400067D RID: 1661
		private bool _dealingPassed;
	}
}
