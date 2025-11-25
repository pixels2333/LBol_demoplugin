using System;
using LBoL.Core.StatusEffects;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class RemoveStatusEffectAction : SimpleEventBattleAction<StatusEffectEventArgs>
	{
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
		protected override void PreEventPhase()
		{
			base.Trigger(base.Args.Unit.StatusEffectRemoving);
		}
		protected override void MainPhase()
		{
			if (!base.Battle.RemoveStatusEffect(base.Args.Unit, base.Args.Effect))
			{
				base.Args.ForceCancelBecause(CancelCause.Failure);
			}
			base.Args.CanCancel = false;
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Args.Unit.StatusEffectRemoved);
		}
	}
}
