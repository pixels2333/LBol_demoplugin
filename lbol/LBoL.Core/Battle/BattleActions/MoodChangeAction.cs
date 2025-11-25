using System;
using System.Collections.Generic;
using LBoL.Base.Extensions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public class MoodChangeAction : EventBattleAction<MoodChangeEventArgs>
	{
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
		private sealed class MoodChangeAchievementCounter : ICustomCounter
		{
			public CustomCounterResetTiming AutoResetTiming
			{
				get
				{
					return CustomCounterResetTiming.PlayerTurnStart;
				}
			}
			public void Increase(BattleController battle)
			{
				this._counter++;
				if (this._counter >= 7 && battle.GameRun.IsAutoSeed && battle.GameRun.JadeBoxes.Empty<JadeBox>())
				{
					battle.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.MoodChange);
				}
			}
			public void Reset(BattleController battle)
			{
				this._counter = 0;
			}
			private int _counter;
		}
	}
}
