using System;
using System.Linq;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public class DamageDealingEventArgs : GameEventArgs
	{
		public Unit Source { get; internal set; }
		public Unit[] Targets { get; internal set; }
		public string GunName { get; internal set; }
		public DamageInfo DamageInfo { get; set; }
		protected override string GetBaseDebugString()
		{
			Unit[] targets = this.Targets;
			if (targets == null || targets.Length != 1)
			{
				return string.Format("{0} --- {1} --> [{2}]", GameEventArgs.DebugString(this.Source), this.DamageInfo, string.Join(", ", Enumerable.Select<Unit, string>(this.Targets, new Func<Unit, string>(GameEventArgs.DebugString))));
			}
			return string.Format("{0} --- {1} --> {2}", GameEventArgs.DebugString(this.Source), this.DamageInfo, GameEventArgs.DebugString(this.Targets[0]));
		}
	}
}
