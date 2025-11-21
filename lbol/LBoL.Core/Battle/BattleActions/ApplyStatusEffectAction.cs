using System;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000161 RID: 353
	public class ApplyStatusEffectAction : SimpleEventBattleAction<StatusEffectApplyEventArgs>
	{
		// Token: 0x06000DD2 RID: 3538 RVA: 0x00025F4C File Offset: 0x0002414C
		public ApplyStatusEffectAction(Type type, Unit target, int? level = null, int? duration = null, int? count = null, int? limit = null, float occupationTime = 0f, bool startAutoDecreasing = true)
		{
			if (!typeof(StatusEffect).IsAssignableFrom(type))
			{
				throw new ArgumentException("<" + type.Name + "> is not StatusEffect");
			}
			StatusEffect statusEffect = Library.CreateStatusEffect(type);
			statusEffect.IsAutoDecreasing = startAutoDecreasing;
			base.Args = new StatusEffectApplyEventArgs
			{
				Unit = target,
				Effect = statusEffect,
				WaitTime = occupationTime
			};
			if (statusEffect.HasLevel)
			{
				if (level == null)
				{
					throw new ArgumentException(type.Name + "'s Level is not provided.");
				}
				int num = Math.Min(level.Value, 9999);
				statusEffect.SetInitLevel(num);
				base.Args.Level = new int?(num);
			}
			if (statusEffect.HasDuration)
			{
				if (duration == null)
				{
					throw new ArgumentException(type.Name + "'s Duration is not provided.");
				}
				int num2 = Math.Min(duration.Value, 9999);
				statusEffect.SetInitDuration(num2);
				base.Args.Duration = new int?(num2);
			}
			if (statusEffect.HasCount && count != null)
			{
				int num3 = Math.Min(count.Value, 9999);
				statusEffect.SetInitCount(num3);
				base.Args.Count = new int?(num3);
			}
			if (limit != null)
			{
				statusEffect.Limit = limit.Value;
			}
		}

		// Token: 0x06000DD3 RID: 3539 RVA: 0x000260AF File Offset: 0x000242AF
		protected override void PreEventPhase()
		{
			base.Trigger(base.Args.Unit.StatusEffectAdding);
		}

		// Token: 0x06000DD4 RID: 3540 RVA: 0x000260C8 File Offset: 0x000242C8
		protected override void MainPhase()
		{
			if (base.Args.Unit.IsInvalidTarget)
			{
				base.Args.ForceCancelBecause(CancelCause.InvalidTarget);
				return;
			}
			StatusEffectAddResult? statusEffectAddResult = base.Battle.TryAddStatusEffect(base.Args.Unit, base.Args.Effect);
			if (statusEffectAddResult == null)
			{
				base.Args.ForceCancelBecause(CancelCause.Failure);
			}
			else
			{
				base.Args.AddResult = statusEffectAddResult;
			}
			base.Args.IsModified = true;
			base.Args.CanCancel = false;
		}

		// Token: 0x06000DD5 RID: 3541 RVA: 0x00026151 File Offset: 0x00024351
		protected override void PostEventPhase()
		{
			base.Trigger(base.Args.Unit.StatusEffectAdded);
		}
	}
}
