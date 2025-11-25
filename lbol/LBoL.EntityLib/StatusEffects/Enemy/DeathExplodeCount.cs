using System;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	public sealed class DeathExplodeCount : DeathExplode
	{
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			base.Count = base.Limit;
			base.HandleOwnerEvent<UnitEventArgs>(base.Owner.TurnEnding, new GameEventHandler<UnitEventArgs>(this.OnTurnEnding));
		}
		private void OnTurnEnding(UnitEventArgs args)
		{
			if (base.Count > 0)
			{
				int num = base.Count - 1;
				base.Count = num;
			}
		}
	}
}
