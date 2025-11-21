using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200016A RID: 362
	public sealed class DamageAction : BattleAction
	{
		// Token: 0x170004D9 RID: 1241
		// (get) Token: 0x06000DF8 RID: 3576 RVA: 0x00026751 File Offset: 0x00024951
		public DamageDealingEventArgs DealingArgs { get; }

		// Token: 0x170004DA RID: 1242
		// (get) Token: 0x06000DF9 RID: 3577 RVA: 0x00026759 File Offset: 0x00024959
		public DamageEventArgs[] DamageArgs { get; }

		// Token: 0x170004DB RID: 1243
		// (get) Token: 0x06000DFA RID: 3578 RVA: 0x00026761 File Offset: 0x00024961
		public string GunName { get; }

		// Token: 0x170004DC RID: 1244
		// (get) Token: 0x06000DFB RID: 3579 RVA: 0x00026769 File Offset: 0x00024969
		public GunType GunType { get; }

		// Token: 0x06000DFC RID: 3580 RVA: 0x00026774 File Offset: 0x00024974
		public DamageAction(Unit source, IEnumerable<Unit> targets, DamageInfo damageInfo, string gunName = "Instant", GunType gunType = GunType.Single)
		{
			Unit[] array = Enumerable.ToArray<Unit>(targets);
			if (Enumerable.Any<Unit>(array, (Unit t) => t == null))
			{
				throw new ArgumentException("Creating DamageAction with null target");
			}
			this.DealingArgs = new DamageDealingEventArgs
			{
				Source = source,
				Targets = array,
				GunName = gunName,
				DamageInfo = damageInfo
			};
			this.DamageArgs = Enumerable.ToArray<DamageEventArgs>(Enumerable.Select<Unit, DamageEventArgs>(array, (Unit t) => new DamageEventArgs
			{
				Source = source,
				Target = t,
				GunName = gunName
			}));
			this.GunName = gunName;
			this.GunType = gunType;
		}

		// Token: 0x06000DFD RID: 3581 RVA: 0x00026837 File Offset: 0x00024A37
		public DamageAction(Unit source, Unit unit, DamageInfo damageInfo, string gunName = "Instant", GunType gunType = GunType.Single)
			: this(source, new Unit[] { unit }, damageInfo, gunName, gunType)
		{
		}

		// Token: 0x06000DFE RID: 3582 RVA: 0x0002684F File Offset: 0x00024A4F
		public static DamageAction LoseLife(Unit target, int life, string gunName = "Instant")
		{
			return new DamageAction(target, target, DamageInfo.HpLose((float)life, false), gunName, GunType.Single);
		}

		// Token: 0x06000DFF RID: 3583 RVA: 0x00026862 File Offset: 0x00024A62
		public static DamageAction Reaction(Unit target, int damage, string gunName = "Instant")
		{
			return new DamageAction(target, target, DamageInfo.Reaction((float)damage, false), gunName, GunType.Single);
		}

		// Token: 0x06000E00 RID: 3584 RVA: 0x00026878 File Offset: 0x00024A78
		public override BattleAction SetCause(ActionCause cause)
		{
			base.SetCause(cause);
			this.DealingArgs.Cause = cause;
			DamageEventArgs[] damageArgs = this.DamageArgs;
			for (int i = 0; i < damageArgs.Length; i++)
			{
				damageArgs[i].Cause = cause;
			}
			return this;
		}

		// Token: 0x06000E01 RID: 3585 RVA: 0x000268B8 File Offset: 0x00024AB8
		public override BattleAction SetSource(GameEntity source)
		{
			base.SetSource(source);
			this.DealingArgs.ActionSource = source;
			DamageEventArgs[] damageArgs = this.DamageArgs;
			for (int i = 0; i < damageArgs.Length; i++)
			{
				damageArgs[i].ActionSource = source;
			}
			return this;
		}

		// Token: 0x170004DD RID: 1245
		// (get) Token: 0x06000E02 RID: 3586 RVA: 0x000268F8 File Offset: 0x00024AF8
		public override bool IsModified
		{
			get
			{
				if (!this._dealingPassed)
				{
					return this.DealingArgs.IsModified;
				}
				return Enumerable.Any<DamageEventArgs>(this.DamageArgs, (DamageEventArgs arg) => arg.IsModified);
			}
		}

		// Token: 0x170004DE RID: 1246
		// (get) Token: 0x06000E03 RID: 3587 RVA: 0x00026938 File Offset: 0x00024B38
		public override string[] Modifiers
		{
			get
			{
				if (!this._dealingPassed)
				{
					return this.DealingArgs.Modifiers;
				}
				return Enumerable.ToArray<string>(Enumerable.Distinct<string>(Enumerable.SelectMany<DamageEventArgs, string>(this.DamageArgs, (DamageEventArgs arg) => arg.Modifiers)));
			}
		}

		// Token: 0x170004DF RID: 1247
		// (get) Token: 0x06000E04 RID: 3588 RVA: 0x0002698D File Offset: 0x00024B8D
		public override bool IsCanceled
		{
			get
			{
				if (!this._dealingPassed)
				{
					return this.DealingArgs.IsCanceled;
				}
				return Enumerable.All<DamageEventArgs>(this.DamageArgs, (DamageEventArgs arg) => arg.IsCanceled);
			}
		}

		// Token: 0x170004E0 RID: 1248
		// (get) Token: 0x06000E05 RID: 3589 RVA: 0x000269D0 File Offset: 0x00024BD0
		public override CancelCause CancelCause
		{
			get
			{
				if (!this._dealingPassed)
				{
					return this.DealingArgs.CancelCause;
				}
				return Enumerable.Aggregate<DamageEventArgs, CancelCause>(this.DamageArgs, CancelCause.None, (CancelCause current, DamageEventArgs args) => current | args.CancelCause);
			}
		}

		// Token: 0x06000E06 RID: 3590 RVA: 0x00026A1C File Offset: 0x00024C1C
		public override void ClearModifiers()
		{
			this.DealingArgs.ClearModifiers();
			DamageEventArgs[] damageArgs = this.DamageArgs;
			for (int i = 0; i < damageArgs.Length; i++)
			{
				damageArgs[i].ClearModifiers();
			}
		}

		// Token: 0x06000E07 RID: 3591 RVA: 0x00026A51 File Offset: 0x00024C51
		internal override IEnumerable<Phase> GetPhases()
		{
			if (Enumerable.All<Unit>(this.DealingArgs.Targets, (Unit t) => t.IsDead))
			{
				Debug.LogWarning("[DamageAction] Targets [" + string.Join(", ", Enumerable.Select<Unit, string>(this.DealingArgs.Targets, (Unit t) => t.Name)) + "] all dead, action is canceled");
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
					damageEventArgs3.Kill = true;
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
			yield break;
			yield break;
		}

		// Token: 0x06000E08 RID: 3592 RVA: 0x00026A64 File Offset: 0x00024C64
		public override string ExportDebugDetails()
		{
			if (!this._dealingPassed)
			{
				return this.DealingArgs.ExportDebugDetails();
			}
			return string.Join(", ", Enumerable.Select<DamageEventArgs, string>(this.DamageArgs, (DamageEventArgs arg) => arg.ExportDebugDetails()));
		}

		// Token: 0x0400065E RID: 1630
		private bool _dealingPassed;
	}
}
