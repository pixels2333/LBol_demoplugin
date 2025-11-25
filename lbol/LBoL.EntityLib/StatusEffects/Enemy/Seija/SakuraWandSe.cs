using System;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits.Seija;
namespace LBoL.EntityLib.StatusEffects.Enemy.Seija
{
	public sealed class SakuraWandSe : SeijaSe
	{
		protected override Type ExhibitType
		{
			get
			{
				return typeof(SakuraWand);
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			this.React(PerformAction.Sfx("GuirenItem", 0f));
			this.React(PerformAction.EffectMessage(unit, "SeijaExhibitManager", "AddExhibit", this));
		}
	}
}
