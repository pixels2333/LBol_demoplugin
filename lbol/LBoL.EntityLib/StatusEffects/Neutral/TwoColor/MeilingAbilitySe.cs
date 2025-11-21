using System;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	// Token: 0x02000043 RID: 67
	public sealed class MeilingAbilitySe : StatusEffect
	{
		// Token: 0x060000D0 RID: 208 RVA: 0x000037CA File Offset: 0x000019CA
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<StatusEffectApplyEventArgs>(base.Owner.StatusEffectAdding, new GameEventHandler<StatusEffectApplyEventArgs>(this.OnStatusEffectAdding));
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x000037EC File Offset: 0x000019EC
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
