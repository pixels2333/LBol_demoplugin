using System;
using System.Collections.Generic;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001B4 RID: 436
	public sealed class UseUsAction : BattleAction
	{
		// Token: 0x1700053B RID: 1339
		// (get) Token: 0x06000F80 RID: 3968 RVA: 0x000298B7 File Offset: 0x00027AB7
		public UsUsingEventArgs Args { get; }

		// Token: 0x06000F81 RID: 3969 RVA: 0x000298BF File Offset: 0x00027ABF
		public UseUsAction(UltimateSkill us, UnitSelector selector, int consumingEnergy)
		{
			this.Args = new UsUsingEventArgs
			{
				Us = us,
				Selector = selector,
				ConsumingPower = consumingEnergy
			};
		}

		// Token: 0x06000F82 RID: 3970 RVA: 0x000298E7 File Offset: 0x00027AE7
		public override BattleAction SetCause(ActionCause cause)
		{
			base.SetCause(cause);
			this.Args.Cause = cause;
			return this;
		}

		// Token: 0x06000F83 RID: 3971 RVA: 0x000298FE File Offset: 0x00027AFE
		public override BattleAction SetSource(GameEntity source)
		{
			base.SetSource(source);
			this.Args.ActionSource = base.Source;
			return this;
		}

		// Token: 0x1700053C RID: 1340
		// (get) Token: 0x06000F84 RID: 3972 RVA: 0x0002991A File Offset: 0x00027B1A
		public override bool IsModified
		{
			get
			{
				return this.Args.IsModified;
			}
		}

		// Token: 0x1700053D RID: 1341
		// (get) Token: 0x06000F85 RID: 3973 RVA: 0x00029927 File Offset: 0x00027B27
		public override string[] Modifiers
		{
			get
			{
				return this.Args.Modifiers;
			}
		}

		// Token: 0x1700053E RID: 1342
		// (get) Token: 0x06000F86 RID: 3974 RVA: 0x00029934 File Offset: 0x00027B34
		public override bool IsCanceled
		{
			get
			{
				return this.Args.IsCanceled;
			}
		}

		// Token: 0x1700053F RID: 1343
		// (get) Token: 0x06000F87 RID: 3975 RVA: 0x00029941 File Offset: 0x00027B41
		public override CancelCause CancelCause
		{
			get
			{
				return this.Args.CancelCause;
			}
		}

		// Token: 0x06000F88 RID: 3976 RVA: 0x0002994E File Offset: 0x00027B4E
		public override void ClearModifiers()
		{
			this.Args.ClearModifiers();
		}

		// Token: 0x06000F89 RID: 3977 RVA: 0x0002995B File Offset: 0x00027B5B
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreateEventPhase<UsUsingEventArgs>("UsUsing", this.Args, base.Battle.UsUsing);
			yield return base.CreatePhase("UsUsing", delegate
			{
				this.Args.CanCancel = false;
				if (!this.Args.Us.TurnRepeatable)
				{
					this.Args.Us.TurnAvailable = false;
				}
				if (!this.Args.Us.BattleRepeatable)
				{
					this.Args.Us.BattleAvailable = false;
				}
				this.Battle.React(new ConsumePowerAction(this.Args.ConsumingPower), this.Args.Us, ActionCause.UsUse);
			}, false);
			List<DamageAction> damageActions = new List<DamageAction>();
			yield return base.CreatePhase("UsAction", delegate
			{
				this.Battle.React(new Reactor(this.Args.Us.GetActions(this.Args.Selector, damageActions)), this.Args.Us, ActionCause.Us);
			}, false);
			if (damageActions.Count > 0)
			{
				yield return base.CreatePhase("Statistics", delegate
				{
					this.Battle.React(new StatisticalTotalDamageAction(damageActions), this.Args.Us, ActionCause.Us);
				}, false);
			}
			base.Battle.GameRun.UltimateUseCount++;
			yield return base.CreateEventPhase<UsUsingEventArgs>("UsUsed", this.Args, base.Battle.UsUsed);
			yield break;
		}

		// Token: 0x06000F8A RID: 3978 RVA: 0x0002996B File Offset: 0x00027B6B
		public override string ExportDebugDetails()
		{
			return this.Args.ExportDebugDetails();
		}
	}
}
