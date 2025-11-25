using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LBoL.Core;
namespace LBoL.Presentation.Units
{
	public class GunHitArgs
	{
		public GunHitArgs(bool sourceIsPlayer, [TupleElementNames(new string[] { "target", "damageInfo" })] IList<ValueTuple<UnitView, DamageInfo>> pairs, string gunName)
		{
			this.SourceIsPlayer = sourceIsPlayer;
			this.Pairs = pairs;
			this.GunName = gunName;
		}
		public bool SourceIsPlayer { get; }
		[TupleElementNames(new string[] { "target", "damageInfo" })]
		public IList<ValueTuple<UnitView, DamageInfo>> Pairs
		{
			[return: TupleElementNames(new string[] { "target", "damageInfo" })]
			get;
		}
		public string GunName { get; }
	}
}
