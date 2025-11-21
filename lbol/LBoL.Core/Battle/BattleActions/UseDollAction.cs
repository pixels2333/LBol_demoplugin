using System;
using System.Collections.Generic;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001B3 RID: 435
	public sealed class UseDollAction : BattleAction
	{
		// Token: 0x17000536 RID: 1334
		// (get) Token: 0x06000F74 RID: 3956 RVA: 0x000297C9 File Offset: 0x000279C9
		public DollUsingEventArgs Args { get; }

		// Token: 0x06000F75 RID: 3957 RVA: 0x000297D1 File Offset: 0x000279D1
		public UseDollAction(Doll doll, UnitSelector selector)
		{
			this.Args = new DollUsingEventArgs
			{
				Doll = doll,
				Selector = selector,
				ConsumingMagic = doll.MagicCost
			};
		}

		// Token: 0x06000F76 RID: 3958 RVA: 0x000297FE File Offset: 0x000279FE
		public UseDollAction(Doll doll, UnitSelector selector, int consumingMagic)
		{
			this.Args = new DollUsingEventArgs
			{
				Doll = doll,
				Selector = selector,
				ConsumingMagic = consumingMagic
			};
		}

		// Token: 0x06000F77 RID: 3959 RVA: 0x00029826 File Offset: 0x00027A26
		public override BattleAction SetCause(ActionCause cause)
		{
			base.SetCause(cause);
			this.Args.Cause = cause;
			return this;
		}

		// Token: 0x06000F78 RID: 3960 RVA: 0x0002983D File Offset: 0x00027A3D
		public override BattleAction SetSource(GameEntity source)
		{
			base.SetSource(source);
			this.Args.ActionSource = base.Source;
			return this;
		}

		// Token: 0x17000537 RID: 1335
		// (get) Token: 0x06000F79 RID: 3961 RVA: 0x00029859 File Offset: 0x00027A59
		public override bool IsModified
		{
			get
			{
				return this.Args.IsModified;
			}
		}

		// Token: 0x17000538 RID: 1336
		// (get) Token: 0x06000F7A RID: 3962 RVA: 0x00029866 File Offset: 0x00027A66
		public override string[] Modifiers
		{
			get
			{
				return this.Args.Modifiers;
			}
		}

		// Token: 0x17000539 RID: 1337
		// (get) Token: 0x06000F7B RID: 3963 RVA: 0x00029873 File Offset: 0x00027A73
		public override bool IsCanceled
		{
			get
			{
				return this.Args.IsCanceled;
			}
		}

		// Token: 0x1700053A RID: 1338
		// (get) Token: 0x06000F7C RID: 3964 RVA: 0x00029880 File Offset: 0x00027A80
		public override CancelCause CancelCause
		{
			get
			{
				return this.Args.CancelCause;
			}
		}

		// Token: 0x06000F7D RID: 3965 RVA: 0x0002988D File Offset: 0x00027A8D
		public override void ClearModifiers()
		{
			this.Args.ClearModifiers();
		}

		// Token: 0x06000F7E RID: 3966 RVA: 0x0002989A File Offset: 0x00027A9A
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreateEventPhase<DollUsingEventArgs>("DollUsing", this.Args, base.Battle.DollUsing);
			yield return base.CreatePhase("DollUsing", delegate
			{
				this.Args.CanCancel = false;
				this.Battle.React(new ConsumeMagicAction(this.Args.Doll), this.Args.Doll, ActionCause.Doll);
			}, false);
			List<DamageAction> damageActions = new List<DamageAction>();
			yield return base.CreatePhase("DollAction", delegate
			{
				this.Battle.React(new Reactor(this.Args.Doll.GetActions(this.Args.Selector, damageActions)), this.Args.Doll, ActionCause.Doll);
			}, false);
			if (damageActions.Count > 0)
			{
				yield return base.CreatePhase("Statistics", delegate
				{
					this.Battle.React(new StatisticalTotalDamageAction(damageActions), this.Args.Doll, ActionCause.Doll);
				}, false);
			}
			yield return base.CreateEventPhase<DollUsingEventArgs>("DollUsed", this.Args, base.Battle.DollUsed);
			yield break;
		}

		// Token: 0x06000F7F RID: 3967 RVA: 0x000298AA File Offset: 0x00027AAA
		public override string ExportDebugDetails()
		{
			return this.Args.ExportDebugDetails();
		}
	}
}
