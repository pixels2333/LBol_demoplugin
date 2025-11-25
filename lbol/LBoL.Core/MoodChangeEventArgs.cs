using System;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public class MoodChangeEventArgs : GameEventArgs
	{
		public Unit Unit { get; internal set; }
		public Mood BeforeMood { get; internal set; }
		public Mood AfterMood { get; internal set; }
		protected override string GetBaseDebugString()
		{
			return string.Concat(new string[]
			{
				GameEventArgs.DebugString(this.Unit),
				": ",
				GameEventArgs.DebugString(this.BeforeMood),
				" -> ",
				GameEventArgs.DebugString(this.AfterMood)
			});
		}
	}
}
