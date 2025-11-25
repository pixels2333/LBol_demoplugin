using System;
using System.Text;
using LBoL.Core.Battle;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public class DieEventArgs : GameEventArgs
	{
		public Unit Source { get; internal set; }
		public Unit Unit { get; internal set; }
		public DieCause DieCause { get; internal set; }
		public GameEntity DieSource { get; internal set; }
		public int Power { get; set; }
		public int BluePoint { get; set; }
		public int Money { get; set; }
		protected override string GetBaseDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder().Append(GameEventArgs.DebugString(this.Unit)).Append(" (by ").Append(GameEventArgs.DebugString(this.Source))
				.Append(", ")
				.Append(this.DieCause)
				.Append(", with ")
				.Append(GameEventArgs.DebugString(this.DieSource));
			if (this.Power > 0)
			{
				stringBuilder.Append(", P: ").Append(this.Power);
			}
			if (this.BluePoint > 0)
			{
				stringBuilder.Append(", B: ").Append(this.BluePoint);
			}
			if (this.Money > 0)
			{
				stringBuilder.Append(", M: ").Append(this.Money);
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}
	}
}
