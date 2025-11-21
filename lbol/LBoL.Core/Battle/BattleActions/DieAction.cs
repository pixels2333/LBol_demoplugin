using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200016B RID: 363
	public sealed class DieAction : BattleAction
	{
		// Token: 0x170004E1 RID: 1249
		// (get) Token: 0x06000E09 RID: 3593 RVA: 0x00026AB9 File Offset: 0x00024CB9
		// (set) Token: 0x06000E0A RID: 3594 RVA: 0x00026AC1 File Offset: 0x00024CC1
		public DieEventArgs[] ArgsList { get; private set; }

		// Token: 0x06000E0B RID: 3595 RVA: 0x00026ACC File Offset: 0x00024CCC
		internal DieAction(IEnumerable<ValueTuple<Unit, DieCause>> dyingUnits, Unit source, GameEntity dieSource)
		{
			this.ArgsList = Enumerable.ToArray<DieEventArgs>(Enumerable.Select<ValueTuple<Unit, DieCause>, DieEventArgs>(dyingUnits, delegate(ValueTuple<Unit, DieCause> pair)
			{
				Unit item = pair.Item1;
				DieCause item2 = pair.Item2;
				return new DieEventArgs
				{
					Unit = item,
					Source = source,
					DieCause = item2,
					DieSource = dieSource
				};
			}));
		}

		// Token: 0x06000E0C RID: 3596 RVA: 0x00026B10 File Offset: 0x00024D10
		internal DieAction(Unit dyingUnit, DieCause dieCause, Unit source, GameEntity dieSource)
		{
			this.ArgsList = new DieEventArgs[]
			{
				new DieEventArgs
				{
					Unit = dyingUnit,
					Source = source,
					DieCause = dieCause,
					DieSource = dieSource
				}
			};
		}

		// Token: 0x06000E0D RID: 3597 RVA: 0x00026B4C File Offset: 0x00024D4C
		public override BattleAction SetCause(ActionCause cause)
		{
			base.SetCause(cause);
			DieEventArgs[] argsList = this.ArgsList;
			for (int i = 0; i < argsList.Length; i++)
			{
				argsList[i].Cause = cause;
			}
			return this;
		}

		// Token: 0x06000E0E RID: 3598 RVA: 0x00026B80 File Offset: 0x00024D80
		public override BattleAction SetSource(GameEntity source)
		{
			base.SetSource(source);
			DieEventArgs[] argsList = this.ArgsList;
			for (int i = 0; i < argsList.Length; i++)
			{
				argsList[i].ActionSource = source;
			}
			return this;
		}

		// Token: 0x06000E0F RID: 3599 RVA: 0x00026BB4 File Offset: 0x00024DB4
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("PreEvent", delegate
			{
				foreach (DieEventArgs dieEventArgs in this.ArgsList)
				{
					dieEventArgs.Unit.Dying.Execute(dieEventArgs);
					if (dieEventArgs.IsCanceled)
					{
						dieEventArgs.Unit.Status = UnitStatus.Alive;
					}
				}
			}, false);
			yield return base.CreatePhase("Main", delegate
			{
				this.ArgsList = Enumerable.ToArray<DieEventArgs>(Enumerable.Where<DieEventArgs>(this.ArgsList, (DieEventArgs arg) => !arg.IsCanceled));
				foreach (DieEventArgs dieEventArgs2 in this.ArgsList)
				{
					EnemyUnit enemyUnit = dieEventArgs2.Unit as EnemyUnit;
					if (enemyUnit != null)
					{
						DieEventArgs dieEventArgs3 = dieEventArgs2;
						DieEventArgs dieEventArgs4 = dieEventArgs2;
						DieEventArgs dieEventArgs5 = dieEventArgs2;
						ValueTuple<int, int, int> valueTuple = base.Battle.GenerateEnemyPoints(enemyUnit);
						dieEventArgs3.Power = valueTuple.Item1;
						dieEventArgs4.BluePoint = valueTuple.Item2;
						dieEventArgs5.Money = valueTuple.Item3;
						enemyUnit.EnemyPointGenerating.Execute(dieEventArgs2);
						base.Battle.EnemyPointGenerating.Execute(dieEventArgs2);
					}
					base.Battle.Die(dieEventArgs2.Unit, dieEventArgs2.Source == dieEventArgs2.Unit, dieEventArgs2.Power, dieEventArgs2.BluePoint, dieEventArgs2.Money);
					dieEventArgs2.IsModified = true;
					dieEventArgs2.CanCancel = false;
				}
			}, true);
			yield return base.CreatePhase("PostEvent", delegate
			{
				foreach (DieEventArgs dieEventArgs6 in this.ArgsList)
				{
					dieEventArgs6.Unit.Died.Execute(dieEventArgs6);
					if (dieEventArgs6.Unit is EnemyUnit)
					{
						base.Battle.EnemyDied.Execute(dieEventArgs6);
					}
				}
			}, false);
			yield return base.CreatePhase("ClearEffects", delegate
			{
				DieEventArgs[] argsList4 = this.ArgsList;
				for (int l = 0; l < argsList4.Length; l++)
				{
					foreach (StatusEffect statusEffect in Enumerable.ToList<StatusEffect>(argsList4[l].Unit.StatusEffects))
					{
						base.React(new RemoveStatusEffectAction(statusEffect, true, 0.1f), null, new ActionCause?(ActionCause.None));
					}
				}
			}, false);
			yield break;
		}

		// Token: 0x170004E2 RID: 1250
		// (get) Token: 0x06000E10 RID: 3600 RVA: 0x00026BC4 File Offset: 0x00024DC4
		public override bool IsModified
		{
			get
			{
				return Enumerable.Any<DieEventArgs>(this.ArgsList, (DieEventArgs arg) => arg.IsModified);
			}
		}

		// Token: 0x170004E3 RID: 1251
		// (get) Token: 0x06000E11 RID: 3601 RVA: 0x00026BF0 File Offset: 0x00024DF0
		public override string[] Modifiers
		{
			get
			{
				return Enumerable.ToArray<string>(Enumerable.SelectMany<DieEventArgs, string>(this.ArgsList, (DieEventArgs arg) => arg.Modifiers));
			}
		}

		// Token: 0x170004E4 RID: 1252
		// (get) Token: 0x06000E12 RID: 3602 RVA: 0x00026C21 File Offset: 0x00024E21
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170004E5 RID: 1253
		// (get) Token: 0x06000E13 RID: 3603 RVA: 0x00026C24 File Offset: 0x00024E24
		public override CancelCause CancelCause
		{
			get
			{
				return CancelCause.None;
			}
		}

		// Token: 0x06000E14 RID: 3604 RVA: 0x00026C28 File Offset: 0x00024E28
		public override void ClearModifiers()
		{
			DieEventArgs[] argsList = this.ArgsList;
			for (int i = 0; i < argsList.Length; i++)
			{
				argsList[i].ClearModifiers();
			}
		}

		// Token: 0x06000E15 RID: 3605 RVA: 0x00026C54 File Offset: 0x00024E54
		public override string ExportDebugDetails()
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			foreach (DieEventArgs dieEventArgs in this.ArgsList)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					stringBuilder.Append("; ");
				}
				stringBuilder.Append(dieEventArgs.ExportDebugDetails());
			}
			return stringBuilder.ToString();
		}
	}
}
