using System;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Neutral.Blue
{
	[UsedImplicitly]
	public sealed class MoreDraw : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.Battle.DrawCardCount += base.Level;
		}
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				base.Battle.DrawCardCount += other.Level;
			}
			return flag;
		}
		protected override void OnRemoved(Unit unit)
		{
			base.Battle.DrawCardCount -= base.Level;
		}
	}
}
