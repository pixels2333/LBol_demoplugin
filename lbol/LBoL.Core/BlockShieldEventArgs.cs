using System;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public class BlockShieldEventArgs : GameEventArgs
	{
		public Unit Source { get; internal set; }
		public Unit Target { get; internal set; }
		public float Block { get; set; }
		public float Shield { get; set; }
		public bool HasBlock { get; set; }
		public bool HasShield { get; set; }
		public BlockShieldType Type { get; set; }
		protected override string GetBaseDebugString()
		{
			if (this.Type != BlockShieldType.Unspecified)
			{
				return string.Format("{0} --- {{B: {1}, S: {2}, Type: {3}}} --> {4}", new object[]
				{
					GameEventArgs.DebugString(this.Source),
					this.Block,
					this.Shield,
					this.Type,
					GameEventArgs.DebugString(this.Target)
				});
			}
			return string.Format("{0} --- {{B: {1}, S: {2}}} --> {3}", new object[]
			{
				GameEventArgs.DebugString(this.Source),
				this.Block,
				this.Shield,
				GameEventArgs.DebugString(this.Target)
			});
		}
	}
}
