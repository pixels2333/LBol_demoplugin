using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	[UsedImplicitly]
	public sealed class RinSummon : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.Count = base.Limit;
			base.HandleOwnerEvent<UnitEventArgs>(base.Owner.TurnEnding, new GameEventHandler<UnitEventArgs>(this.OnOwnerTurnEnding));
		}
		private void OnOwnerTurnEnding(UnitEventArgs args)
		{
			if (base.Count > 0)
			{
				int num = base.Count - 1;
				base.Count = num;
				this.NotifyChanged();
			}
			if (base.Count <= 0 && base.Level < 3)
			{
				int num = base.Level + 1;
				base.Level = num;
				base.Count = base.Limit;
				base.NotifyActivating();
			}
		}
		private const int MaxLevel = 3;
	}
}
