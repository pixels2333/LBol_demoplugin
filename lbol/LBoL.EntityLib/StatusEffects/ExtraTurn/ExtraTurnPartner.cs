using System;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.StatusEffects.ExtraTurn
{
	public abstract class ExtraTurnPartner : StatusEffect
	{
		protected bool ThisTurnActivating { get; set; }
		protected override string GetBaseDescription()
		{
			if (!this.ThisTurnActivating)
			{
				return base.ExtraDescription;
			}
			return base.GetBaseDescription();
		}
	}
}
