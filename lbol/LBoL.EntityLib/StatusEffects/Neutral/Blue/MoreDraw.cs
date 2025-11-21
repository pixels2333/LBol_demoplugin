using System;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.Blue
{
	// Token: 0x0200005D RID: 93
	[UsedImplicitly]
	public sealed class MoreDraw : StatusEffect
	{
		// Token: 0x06000143 RID: 323 RVA: 0x00004783 File Offset: 0x00002983
		protected override void OnAdded(Unit unit)
		{
			base.Battle.DrawCardCount += base.Level;
		}

		// Token: 0x06000144 RID: 324 RVA: 0x0000479D File Offset: 0x0000299D
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				base.Battle.DrawCardCount += other.Level;
			}
			return flag;
		}

		// Token: 0x06000145 RID: 325 RVA: 0x000047C1 File Offset: 0x000029C1
		protected override void OnRemoved(Unit unit)
		{
			base.Battle.DrawCardCount -= base.Level;
		}
	}
}
