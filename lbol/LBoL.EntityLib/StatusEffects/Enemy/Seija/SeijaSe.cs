using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Enemy.Seija
{
	[UsedImplicitly]
	public class SeijaSe : StatusEffect
	{
		protected virtual Type ExhibitType
		{
			get
			{
				return null;
			}
		}
		protected override void OnAdded(Unit unit)
		{
			if (this.ExhibitType != null)
			{
				Exhibit exhibit = Library.CreateExhibit(this.ExhibitType);
				base.GameRun.RevealExhibit(exhibit);
			}
		}
	}
}
