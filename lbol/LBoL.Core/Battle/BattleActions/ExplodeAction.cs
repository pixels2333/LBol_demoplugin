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
	public sealed class ExplodeAction : BattleAction
	{
		public DieEventArgs DieArgs { get; }
		public DamageDealingEventArgs DealingArgs { get; }
		public DamageEventArgs[] DamageArgs { get; }
		public string GunName { get; }
		public GunType GunType { get; }
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
		public ExplodeAction(EnemyUnit unit, Unit damageTarget, DamageInfo damageInfo, DieCause dieCause, GameEntity dieSource, string gunName = "Instant", GunType gunType = GunType.Single)
			: this(unit, new Unit[] { damageTarget }, damageInfo, dieCause, dieSource, gunName, gunType)
		{
		}
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
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}
		public override CancelCause CancelCause
		{
			get
			{
				return CancelCause.None;
			}
		}
		public override void ClearModifiers()
		{
			this.DieArgs.ClearModifiers();
			DamageEventArgs[] damageArgs = this.DamageArgs;
			for (int i = 0; i < damageArgs.Length; i++)
			{
				damageArgs[i].ClearModifiers();
			}
		}
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
		public override string ExportDebugDetails()
		{
			if (!this._dealingPassed)
			{
				return "Damage: " + this.DealingArgs.ExportDebugDetails() + "; Die: " + this.DieArgs.ExportDebugDetails();
			}
			return "Damage: [" + string.Join(", ", Enumerable.Select<DamageEventArgs, string>(this.DamageArgs, (DamageEventArgs a) => a.ExportDebugDetails())) + "]; Die: " + this.DieArgs.ExportDebugDetails();
		}
		private bool _dealingPassed;
	}
}
