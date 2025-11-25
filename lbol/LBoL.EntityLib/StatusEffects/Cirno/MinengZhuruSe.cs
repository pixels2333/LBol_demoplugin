using System;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Cirno
{
	[UsedImplicitly]
	public sealed class MinengZhuruSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.Battle.FriendPassiveTimes += base.Level;
		}
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				base.Battle.FriendPassiveTimes += other.Level;
			}
			return flag;
		}
		protected override void OnRemoved(Unit unit)
		{
			base.Battle.FriendPassiveTimes = Math.Max(1, base.Battle.FriendPassiveTimes - base.Level);
		}
	}
}
