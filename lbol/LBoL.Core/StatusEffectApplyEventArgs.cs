using System;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public class StatusEffectApplyEventArgs : GameEventArgs
	{
		public Unit Unit { get; internal set; }
		public StatusEffect Effect { get; internal set; }
		public int? Level { get; set; }
		public int? Duration { get; set; }
		public int? Count { get; set; }
		public StatusEffectAddResult? AddResult { get; internal set; }
		public float WaitTime { get; set; }
		protected override string GetBaseDebugString()
		{
			StatusEffectAddResult? addResult = this.AddResult;
			if (addResult != null)
			{
				StatusEffectAddResult valueOrDefault = addResult.GetValueOrDefault();
				return string.Format("{0} -> {1} (result: {2})", GameEventArgs.DebugString(this.Effect), GameEventArgs.DebugString(this.Unit), valueOrDefault);
			}
			return GameEventArgs.DebugString(this.Effect) + " -> " + GameEventArgs.DebugString(this.Unit);
		}
	}
}
