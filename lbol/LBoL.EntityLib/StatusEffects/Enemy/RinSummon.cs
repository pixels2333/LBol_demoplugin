using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000C1 RID: 193
	[UsedImplicitly]
	public sealed class RinSummon : StatusEffect
	{
		// Token: 0x060002A2 RID: 674 RVA: 0x0000743B File Offset: 0x0000563B
		protected override void OnAdded(Unit unit)
		{
			base.Count = base.Limit;
			base.HandleOwnerEvent<UnitEventArgs>(base.Owner.TurnEnding, new GameEventHandler<UnitEventArgs>(this.OnOwnerTurnEnding));
		}

		// Token: 0x060002A3 RID: 675 RVA: 0x00007468 File Offset: 0x00005668
		private void OnOwnerTurnEnding(UnitEventArgs args)
		{
			if (base.Count > 0)
			{
				int num = base.Count - 1;
				base.Count = num;
				this.NotifyChanged();
			}
			if (base.Count <= 0 && base.Level < 3)
			{
				int num = base.Level + 1;
				base.Level = num;
				base.Count = base.Limit;
				base.NotifyActivating();
			}
		}

		// Token: 0x0400001E RID: 30
		private const int MaxLevel = 3;
	}
}
