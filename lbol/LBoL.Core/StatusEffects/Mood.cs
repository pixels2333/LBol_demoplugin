using System;
using System.Linq;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x020000A4 RID: 164
	public abstract class Mood : StatusEffect
	{
		// Token: 0x06000791 RID: 1937 RVA: 0x000164B8 File Offset: 0x000146B8
		protected override void OnAdding(Unit unit)
		{
			Mood mood = (Mood)Enumerable.FirstOrDefault<StatusEffect>(base.Battle.Player.StatusEffects, (StatusEffect se) => se is Mood);
			if (mood != null)
			{
				this.React(new MoodChangeAction(base.Battle.Player, mood, this));
				this._moodChangeHappen = true;
			}
		}

		// Token: 0x06000792 RID: 1938 RVA: 0x00016526 File Offset: 0x00014726
		protected override void OnAdded(Unit unit)
		{
			if (!this._moodChangeHappen)
			{
				this.React(new MoodChangeAction(base.Battle.Player, null, this));
				this._moodChangeHappen = true;
			}
		}

		// Token: 0x04000354 RID: 852
		private bool _moodChangeHappen;
	}
}
