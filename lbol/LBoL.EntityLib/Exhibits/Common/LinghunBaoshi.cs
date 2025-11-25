using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Stations;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class LinghunBaoshi : Exhibit
	{
		public override string OverrideIconName
		{
			get
			{
				if (base.Counter != 0)
				{
					return base.Id + "Inactive";
				}
				return base.Id;
			}
		}
		public override bool ShowCounter
		{
			get
			{
				return false;
			}
		}
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<StationEventArgs>(base.GameRun.StationEntered, delegate(StationEventArgs args)
			{
				if (args.Station.Type == StationType.Gap)
				{
					base.Counter = 0;
				}
			});
		}
		protected override void OnGain(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			bool flag;
			if (gameRun == null)
			{
				flag = false;
			}
			else
			{
				Station currentStation = gameRun.CurrentStation;
				StationType? stationType = ((currentStation != null) ? new StationType?(currentStation.Type) : default(StationType?));
				StationType stationType2 = StationType.Gap;
				flag = (stationType.GetValueOrDefault() == stationType2) & (stationType != null);
			}
			base.Counter = (flag ? 0 : 1);
		}
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Counter == 0)
			{
				base.NotifyActivating();
				yield return new GainManaAction(base.Mana);
				base.Counter = 1;
			}
			base.Blackout = true;
			yield break;
		}
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
