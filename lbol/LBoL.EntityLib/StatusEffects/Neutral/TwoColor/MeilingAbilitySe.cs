using System;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	public sealed class MeilingAbilitySe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<StatusEffectApplyEventArgs>(base.Owner.StatusEffectAdding, new GameEventHandler<StatusEffectApplyEventArgs>(this.OnStatusEffectAdding));
		}
		private void OnStatusEffectAdding(StatusEffectApplyEventArgs args)
		{
			StatusEffect effect = args.Effect;
			if (effect is Firepower || effect is TempFirepower)
			{
				base.NotifyActivating();
				args.Effect.Level *= 2;
			}
		}
	}
}
