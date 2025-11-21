using System;
using System.Collections.Generic;
using LBoL.Base.Extensions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000193 RID: 403
	public class MoodChangeAction : EventBattleAction<MoodChangeEventArgs>
	{
		// Token: 0x06000ED5 RID: 3797 RVA: 0x000281FC File Offset: 0x000263FC
		public MoodChangeAction(Unit unit, Mood before, Mood after)
		{
			base.Args = new MoodChangeEventArgs
			{
				Unit = unit,
				BeforeMood = before,
				AfterMood = after
			};
			if (base.Args.Unit == null)
			{
				base.Args.ForceCancelBecause(CancelCause.InvalidTarget);
			}
		}

		// Token: 0x06000ED6 RID: 3798 RVA: 0x00028248 File Offset: 0x00026448
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreateEventPhase<MoodChangeEventArgs>("MoodChanging", base.Args, base.Args.Unit.MoodChanging);
			if (base.Args.BeforeMood != null)
			{
				yield return base.CreatePhase("LeaveMood", delegate
				{
					base.React(new RemoveStatusEffectAction(base.Args.BeforeMood, true, 0.1f), null, default(ActionCause?));
				}, false);
			}
			base.Battle.IncreaseCounter<MoodChangeAction.MoodChangeAchievementCounter>();
			yield return base.CreateEventPhase<MoodChangeEventArgs>("MoodChanged", base.Args, base.Args.Unit.MoodChanged);
			yield break;
		}

		// Token: 0x020002E2 RID: 738
		private sealed class MoodChangeAchievementCounter : ICustomCounter
		{
			// Token: 0x17000646 RID: 1606
			// (get) Token: 0x06001581 RID: 5505 RVA: 0x0003CEE3 File Offset: 0x0003B0E3
			public CustomCounterResetTiming AutoResetTiming
			{
				get
				{
					return CustomCounterResetTiming.PlayerTurnStart;
				}
			}

			// Token: 0x06001582 RID: 5506 RVA: 0x0003CEE8 File Offset: 0x0003B0E8
			public void Increase(BattleController battle)
			{
				this._counter++;
				if (this._counter >= 7 && battle.GameRun.IsAutoSeed && battle.GameRun.JadeBoxes.Empty<JadeBox>())
				{
					battle.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.MoodChange);
				}
			}

			// Token: 0x06001583 RID: 5507 RVA: 0x0003CF3D File Offset: 0x0003B13D
			public void Reset(BattleController battle)
			{
				this._counter = 0;
			}

			// Token: 0x04000B85 RID: 2949
			private int _counter;
		}
	}
}
