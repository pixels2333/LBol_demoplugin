using System;
using System.Collections.Generic;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000177 RID: 375
	public sealed class EndEnemyTurnAction : BattleAction
	{
		// Token: 0x170004F1 RID: 1265
		// (get) Token: 0x06000E47 RID: 3655 RVA: 0x00027266 File Offset: 0x00025466
		public UnitEventArgs Args { get; }

		// Token: 0x170004F2 RID: 1266
		// (get) Token: 0x06000E48 RID: 3656 RVA: 0x0002726E File Offset: 0x0002546E
		public EnemyUnit Unit { get; }

		// Token: 0x06000E49 RID: 3657 RVA: 0x00027276 File Offset: 0x00025476
		internal EndEnemyTurnAction(EnemyUnit enemy)
		{
			this.Unit = enemy;
			this.Args = new UnitEventArgs
			{
				Unit = enemy,
				CanCancel = false
			};
		}

		// Token: 0x06000E4A RID: 3658 RVA: 0x0002729E File Offset: 0x0002549E
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("OutTurn", delegate
			{
				this.Unit.IsInTurn = false;
			}, false);
			yield return base.CreateEventPhase<UnitEventArgs>("TurnEnding", this.Args, this.Args.Unit.TurnEnding);
			yield return base.CreatePhase("Main", delegate
			{
				base.Battle.EndEnemyTurn(this.Unit);
			}, true);
			yield return base.CreatePhase("DecreaseDuration", delegate
			{
				base.Battle.TurnEndDecreaseDuration(this.Unit);
			}, false);
			yield return base.CreateEventPhase<UnitEventArgs>("TurnEnded", this.Args, this.Args.Unit.TurnEnded);
			yield break;
		}

		// Token: 0x170004F3 RID: 1267
		// (get) Token: 0x06000E4B RID: 3659 RVA: 0x000272AE File Offset: 0x000254AE
		public override bool IsModified
		{
			get
			{
				return this.Args.IsModified;
			}
		}

		// Token: 0x170004F4 RID: 1268
		// (get) Token: 0x06000E4C RID: 3660 RVA: 0x000272BB File Offset: 0x000254BB
		public override string[] Modifiers
		{
			get
			{
				return this.Args.Modifiers;
			}
		}

		// Token: 0x170004F5 RID: 1269
		// (get) Token: 0x06000E4D RID: 3661 RVA: 0x000272C8 File Offset: 0x000254C8
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170004F6 RID: 1270
		// (get) Token: 0x06000E4E RID: 3662 RVA: 0x000272CB File Offset: 0x000254CB
		public override CancelCause CancelCause
		{
			get
			{
				return CancelCause.None;
			}
		}

		// Token: 0x06000E4F RID: 3663 RVA: 0x000272CE File Offset: 0x000254CE
		public override void ClearModifiers()
		{
			this.Args.ClearModifiers();
		}

		// Token: 0x06000E50 RID: 3664 RVA: 0x000272DB File Offset: 0x000254DB
		public override string ExportDebugDetails()
		{
			return this.Args.ExportDebugDetails();
		}
	}
}
