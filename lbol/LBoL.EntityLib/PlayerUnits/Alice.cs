using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.EntityLib.PlayerUnits
{
	// Token: 0x02000105 RID: 261
	[UsedImplicitly]
	public sealed class Alice : PlayerUnit
	{
		// Token: 0x17000063 RID: 99
		// (get) Token: 0x060003A3 RID: 931 RVA: 0x0000A4EE File Offset: 0x000086EE
		public override bool ShowDollSlotByDefault
		{
			get
			{
				return true;
			}
		}
	}
}
