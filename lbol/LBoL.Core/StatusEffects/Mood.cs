using System;
using System.Linq;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	public abstract class Mood : StatusEffect
	{
		protected override void OnAdding(Unit unit)
		{
			Mood mood = (Mood)Enumerable.FirstOrDefault<StatusEffect>(base.Battle.Player.StatusEffects, (StatusEffect se) => se is Mood);
			if (mood != null)
			{
				this.React(new MoodChangeAction(base.Battle.Player, mood, this));
				this._moodChangeHappen = true;
			}
		}
		protected override void OnAdded(Unit unit)
		{
			if (!this._moodChangeHappen)
			{
				this.React(new MoodChangeAction(base.Battle.Player, null, this));
				this._moodChangeHappen = true;
			}
		}
		private bool _moodChangeHappen;
	}
}
