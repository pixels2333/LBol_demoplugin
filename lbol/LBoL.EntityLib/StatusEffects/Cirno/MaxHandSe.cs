using System;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Cirno
{
	[UsedImplicitly]
	public sealed class MaxHandSe : StatusEffect
	{
		[UsedImplicitly]
		public int MaxHand
		{
			get
			{
				return 12;
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.Battle.MaxHand = this.MaxHand;
		}
	}
}
