using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LBoL.Core;

namespace LBoL.Presentation.Units
{
	// Token: 0x02000019 RID: 25
	public class GunHitArgs
	{
		// Token: 0x0600020E RID: 526 RVA: 0x0000A254 File Offset: 0x00008454
		public GunHitArgs(bool sourceIsPlayer, [TupleElementNames(new string[] { "target", "damageInfo" })] IList<ValueTuple<UnitView, DamageInfo>> pairs, string gunName)
		{
			this.SourceIsPlayer = sourceIsPlayer;
			this.Pairs = pairs;
			this.GunName = gunName;
		}

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x0600020F RID: 527 RVA: 0x0000A271 File Offset: 0x00008471
		public bool SourceIsPlayer { get; }

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x06000210 RID: 528 RVA: 0x0000A279 File Offset: 0x00008479
		[TupleElementNames(new string[] { "target", "damageInfo" })]
		public IList<ValueTuple<UnitView, DamageInfo>> Pairs
		{
			[return: TupleElementNames(new string[] { "target", "damageInfo" })]
			get;
		}

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x06000211 RID: 529 RVA: 0x0000A281 File Offset: 0x00008481
		public string GunName { get; }
	}
}
