using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Cirno
{
	// Token: 0x020000D7 RID: 215
	[UsedImplicitly]
	public sealed class ColdHeartedSe : StatusEffect
	{
		// Token: 0x06000306 RID: 774 RVA: 0x0000831E File Offset: 0x0000651E
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<UnitEventArgs>(unit.TurnEnded, delegate(UnitEventArgs _)
			{
				this.React(new RemoveStatusEffectAction(this, true, 0.1f));
			});
		}
	}
}
