using System;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Cirno
{
	// Token: 0x020000E5 RID: 229
	[UsedImplicitly]
	public sealed class MinengZhuruSe : StatusEffect
	{
		// Token: 0x06000335 RID: 821 RVA: 0x0000890A File Offset: 0x00006B0A
		protected override void OnAdded(Unit unit)
		{
			base.Battle.FriendPassiveTimes += base.Level;
		}

		// Token: 0x06000336 RID: 822 RVA: 0x00008924 File Offset: 0x00006B24
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				base.Battle.FriendPassiveTimes += other.Level;
			}
			return flag;
		}

		// Token: 0x06000337 RID: 823 RVA: 0x00008948 File Offset: 0x00006B48
		protected override void OnRemoved(Unit unit)
		{
			base.Battle.FriendPassiveTimes = Math.Max(1, base.Battle.FriendPassiveTimes - base.Level);
		}
	}
}
