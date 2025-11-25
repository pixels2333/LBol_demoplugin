using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Marisa;
namespace LBoL.EntityLib.StatusEffects.Marisa
{
	public sealed class UpgradePotionSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnEnded));
		}
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
