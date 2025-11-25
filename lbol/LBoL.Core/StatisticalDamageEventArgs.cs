using System;
using System.Collections.Generic;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public class StatisticalDamageEventArgs : GameEventArgs
	{
		public Unit DamageSource { get; internal set; }
		public IReadOnlyDictionary<Unit, IReadOnlyList<DamageEventArgs>> ArgsTable { get; internal set; }
	}
}
