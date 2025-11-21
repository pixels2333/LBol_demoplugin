using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Marisa;

namespace LBoL.EntityLib.StatusEffects.Marisa
{
	// Token: 0x02000071 RID: 113
	public sealed class UpgradePotionSe : StatusEffect
	{
		// Token: 0x06000186 RID: 390 RVA: 0x00005036 File Offset: 0x00003236
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnEnded));
		}

		// Token: 0x06000187 RID: 391 RVA: 0x0000505C File Offset: 0x0000325C
		private void OnPlayerTurnEnded(UnitEventArgs args)
		{
			List<Potion> list = Enumerable.ToList<Potion>(Enumerable.OfType<Potion>(base.Battle.DrawZone));
			list.AddRange(Enumerable.OfType<Potion>(base.Battle.DiscardZone));
			if (list.Count > 0)
			{
				base.NotifyActivating();
				foreach (Potion potion in list)
				{
					potion.DeltaDamage += base.Level;
				}
			}
		}
	}
}
