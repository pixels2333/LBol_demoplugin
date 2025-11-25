using System;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public class StatusEffectEventArgs : GameEventArgs
	{
		public Unit Unit { get; internal set; }
		public StatusEffect Effect { get; internal set; }
		public float WaitTime { get; set; }
		protected override string GetBaseDebugString()
		{
			return GameEventArgs.DebugString(this.Effect) + " -> " + GameEventArgs.DebugString(this.Unit);
		}
	}
}
