using System;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	public sealed class KokoroDarkPower : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<StatusEffectApplyEventArgs>(base.Battle.Player.StatusEffectAdded, new GameEventHandler<StatusEffectApplyEventArgs>(this.OnPlayerStatusEffectAdded));
		}
		private void OnPlayerStatusEffectAdded(StatusEffectApplyEventArgs args)
		{
			if (args.Effect.Type == StatusEffectType.Negative)
			{
				base.Count++;
			}
		}
	}
}
