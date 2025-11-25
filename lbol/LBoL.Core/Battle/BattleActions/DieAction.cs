using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class DieAction : BattleAction
	{
		public DieEventArgs[] ArgsList { get; private set; }
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
		public override bool IsModified
		{
			get
			{
				return Enumerable.Any<DieEventArgs>(this.ArgsList, (DieEventArgs arg) => arg.IsModified);
			}
		}
		public override string[] Modifiers
		{
			get
			{
				return Enumerable.ToArray<string>(Enumerable.SelectMany<DieEventArgs, string>(this.ArgsList, (DieEventArgs arg) => arg.Modifiers));
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
			DieEventArgs[] argsList = this.ArgsList;
			for (int i = 0; i < argsList.Length; i++)
			{
				argsList[i].ClearModifiers();
			}
		}
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
