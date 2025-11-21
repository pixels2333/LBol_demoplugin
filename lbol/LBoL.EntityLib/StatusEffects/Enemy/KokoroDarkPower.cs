using System;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000AC RID: 172
	public sealed class KokoroDarkPower : StatusEffect
	{
		// Token: 0x06000265 RID: 613 RVA: 0x00006E6C File Offset: 0x0000506C
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<StatusEffectApplyEventArgs>(base.Battle.Player.StatusEffectAdded, new GameEventHandler<StatusEffectApplyEventArgs>(this.OnPlayerStatusEffectAdded));
		}

		// Token: 0x06000266 RID: 614 RVA: 0x00006E90 File Offset: 0x00005090
		private void OnPlayerStatusEffectAdded(StatusEffectApplyEventArgs args)
		{
			if (args.Effect.Type == StatusEffectType.Negative)
			{
				base.Count++;
			}
		}
	}
}
