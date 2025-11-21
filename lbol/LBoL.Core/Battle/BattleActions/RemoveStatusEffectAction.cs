using System;
using LBoL.Core.StatusEffects;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200019D RID: 413
	public sealed class RemoveStatusEffectAction : SimpleEventBattleAction<StatusEffectEventArgs>
	{
		// Token: 0x06000F12 RID: 3858 RVA: 0x00028AF4 File Offset: 0x00026CF4
		public RemoveStatusEffectAction(StatusEffect effect, bool forced = true, float occupationTime = 0.1f)
		{
			base.Args = new StatusEffectEventArgs
			{
				Effect = effect,
				Unit = effect.Owner,
				CanCancel = !forced,
				WaitTime = occupationTime
			};
			if (base.Args.Unit == null)
			{
				base.Args.ForceCancelBecause(CancelCause.InvalidTarget);
			}
		}

		// Token: 0x06000F13 RID: 3859 RVA: 0x00028B4F File Offset: 0x00026D4F
		protected override void PreEventPhase()
		{
			base.Trigger(base.Args.Unit.StatusEffectRemoving);
		}

		// Token: 0x06000F14 RID: 3860 RVA: 0x00028B67 File Offset: 0x00026D67
		protected override void MainPhase()
		{
			if (!base.Battle.RemoveStatusEffect(base.Args.Unit, base.Args.Effect))
			{
				base.Args.ForceCancelBecause(CancelCause.Failure);
			}
			base.Args.CanCancel = false;
		}

		// Token: 0x06000F15 RID: 3861 RVA: 0x00028BA4 File Offset: 0x00026DA4
		protected override void PostEventPhase()
		{
			base.Trigger(base.Args.Unit.StatusEffectRemoved);
		}
	}
}
