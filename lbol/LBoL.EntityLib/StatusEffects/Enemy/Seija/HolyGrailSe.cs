using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits.Seija;
namespace LBoL.EntityLib.StatusEffects.Enemy.Seija
{
	public sealed class HolyGrailSe : SeijaSe
	{
		protected override Type ExhibitType
		{
			get
			{
				return typeof(HolyGrail);
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			this._happened = false;
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
			this.React(PerformAction.Sfx("GuirenItem", 0f));
			this.React(PerformAction.EffectMessage(unit, "SeijaExhibitManager", "AddExhibit", this));
		}
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
		private bool _happened;
	}
}
