using System;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits.Seija;

namespace LBoL.EntityLib.StatusEffects.Enemy.Seija
{
	// Token: 0x020000D1 RID: 209
	public sealed class SakuraWandSe : SeijaSe
	{
		// Token: 0x1700004C RID: 76
		// (get) Token: 0x060002DF RID: 735 RVA: 0x00007D11 File Offset: 0x00005F11
		protected override Type ExhibitType
		{
			get
			{
				return typeof(SakuraWand);
			}
		}

		// Token: 0x060002E0 RID: 736 RVA: 0x00007D1D File Offset: 0x00005F1D
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			this.React(PerformAction.Sfx("GuirenItem", 0f));
			this.React(PerformAction.EffectMessage(unit, "SeijaExhibitManager", "AddExhibit", this));
		}
	}
}
