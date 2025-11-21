using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy.Seija
{
	// Token: 0x020000D2 RID: 210
	[UsedImplicitly]
	public class SeijaSe : StatusEffect
	{
		// Token: 0x1700004D RID: 77
		// (get) Token: 0x060002E2 RID: 738 RVA: 0x00007D64 File Offset: 0x00005F64
		protected virtual Type ExhibitType
		{
			get
			{
				return null;
			}
		}

		// Token: 0x060002E3 RID: 739 RVA: 0x00007D68 File Offset: 0x00005F68
		protected override void OnAdded(Unit unit)
		{
			if (this.ExhibitType != null)
			{
				Exhibit exhibit = Library.CreateExhibit(this.ExhibitType);
				base.GameRun.RevealExhibit(exhibit);
			}
		}
	}
}
