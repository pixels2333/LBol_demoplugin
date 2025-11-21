using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits.Seija;

namespace LBoL.EntityLib.StatusEffects.Enemy.Seija
{
	// Token: 0x020000CD RID: 205
	public sealed class HolyGrailSe : SeijaSe
	{
		// Token: 0x17000046 RID: 70
		// (get) Token: 0x060002C8 RID: 712 RVA: 0x000078FE File Offset: 0x00005AFE
		protected override Type ExhibitType
		{
			get
			{
				return typeof(HolyGrail);
			}
		}

		// Token: 0x060002C9 RID: 713 RVA: 0x0000790C File Offset: 0x00005B0C
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			this._happened = false;
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
			this.React(PerformAction.Sfx("GuirenItem", 0f));
			this.React(PerformAction.EffectMessage(unit, "SeijaExhibitManager", "AddExhibit", this));
		}

		// Token: 0x060002CA RID: 714 RVA: 0x0000797A File Offset: 0x00005B7A
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd || this._happened || !base.Owner.IsAlive)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return PerformAction.Animation(base.Owner, "skill1", 0.8f, null, 0f, -1);
			yield return new LoseAllExhibitsAction();
			this._happened = true;
			yield break;
		}

		// Token: 0x04000020 RID: 32
		private bool _happened;
	}
}
