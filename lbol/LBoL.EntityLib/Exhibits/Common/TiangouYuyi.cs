using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3, ExpireStationLevel = 9)]
	public sealed class TiangouYuyi : Exhibit, IMapModeOverrider
	{
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.AddMapModeOverrider(this);
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.RemoveMapModeOverrider(this);
		}
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarted(GameEventArgs args)
		{
			if (base.Battle.Player.TurnCounter == 1)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Graze>(base.Owner, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
				base.Blackout = true;
			}
			yield break;
		}
		public GameRunMapMode? MapMode
		{
			get
			{
				if (base.Counter <= 0)
				{
					return default(GameRunMapMode?);
				}
				return new GameRunMapMode?(GameRunMapMode.Crossing);
			}
		}
		public void OnEnteredWithMode()
		{
			int num = base.Counter - 1;
			base.Counter = num;
			base.NotifyActivating();
			this.NotifyChanged();
		}
	}
}
