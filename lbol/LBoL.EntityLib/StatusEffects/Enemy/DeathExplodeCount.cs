using System;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x02000092 RID: 146
	public sealed class DeathExplodeCount : DeathExplode
	{
		// Token: 0x06000214 RID: 532 RVA: 0x00006557 File Offset: 0x00004757
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			base.Count = base.Limit;
			base.HandleOwnerEvent<UnitEventArgs>(base.Owner.TurnEnding, new GameEventHandler<UnitEventArgs>(this.OnTurnEnding));
		}

		// Token: 0x06000215 RID: 533 RVA: 0x0000658C File Offset: 0x0000478C
		private void OnTurnEnding(UnitEventArgs args)
		{
			if (base.Count > 0)
			{
				int num = base.Count - 1;
				base.Count = num;
			}
		}
	}
}
