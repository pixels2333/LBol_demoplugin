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
	// Token: 0x0200017B RID: 379
	[UsedImplicitly]
	public sealed class LinghunBaoshi : Exhibit
	{
		// Token: 0x17000078 RID: 120
		// (get) Token: 0x06000549 RID: 1353 RVA: 0x0000D06F File Offset: 0x0000B26F
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

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x0600054A RID: 1354 RVA: 0x0000D090 File Offset: 0x0000B290
		public override bool ShowCounter
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600054B RID: 1355 RVA: 0x0000D093 File Offset: 0x0000B293
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

		// Token: 0x0600054C RID: 1356 RVA: 0x0000D0B4 File Offset: 0x0000B2B4
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

		// Token: 0x0600054D RID: 1357 RVA: 0x0000D10D File Offset: 0x0000B30D
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x0600054E RID: 1358 RVA: 0x0000D131 File Offset: 0x0000B331
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

		// Token: 0x0600054F RID: 1359 RVA: 0x0000D141 File Offset: 0x0000B341
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
