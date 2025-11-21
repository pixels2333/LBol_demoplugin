using System;
using LBoL.Core;
using LBoL.Core.Adventures;
using UnityEngine;

namespace LBoL.EntityLib.Adventures.Common
{
	// Token: 0x02000523 RID: 1315
	[AdventureInfo(WeighterType = typeof(YorigamiSisters.YorigamiSistersWeighter))]
	public sealed class YorigamiSisters : Adventure
	{
		// Token: 0x02000A80 RID: 2688
		private class YorigamiSistersWeighter : IAdventureWeighter
		{
			// Token: 0x060037A3 RID: 14243 RVA: 0x00087246 File Offset: 0x00085446
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return Mathf.Clamp01((float)gameRun.Money / 200f - 1f);
			}
		}
	}
}
