using System;
using System.Collections;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.EntityLib.Exhibits.Adventure
{
	// Token: 0x020001C9 RID: 457
	[UsedImplicitly]
	public sealed class WaijieYanshuang : Exhibit
	{
		// Token: 0x17000088 RID: 136
		// (get) Token: 0x0600069D RID: 1693 RVA: 0x0000F0BC File Offset: 0x0000D2BC
		public override bool ShowCounter
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600069E RID: 1694 RVA: 0x0000F0C0 File Offset: 0x0000D2C0
		protected override void OnAdded(PlayerUnit player)
		{
			if (player.HasExhibit<JingjieGanzhiyi>())
			{
				int counter = base.Counter;
				base.Counter = counter + 1;
			}
			base.HandleGameRunEvent<StationEventArgs>(base.GameRun.StationEntering, delegate(StationEventArgs args)
			{
				GapStation gapStation = args.Station as GapStation;
				if (gapStation != null && base.Counter == 0)
				{
					int num = base.Counter + 1;
					base.Counter = num;
					gapStation.PostDialogs.Add(new StationDialogSource("YukariProvide", new WaijieYanshuang.YanshuangCommandHandler(base.GameRun)));
				}
			});
		}

		// Token: 0x0600069F RID: 1695 RVA: 0x0000F104 File Offset: 0x0000D304
		protected override void OnLeaveBattle()
		{
			PlayerUnit owner = base.Owner;
			if (owner != null && owner.IsAlive)
			{
				base.NotifyActivating();
				base.GameRun.Heal(base.Value1, true, null);
			}
		}

		// Token: 0x02000680 RID: 1664
		private class YanshuangCommandHandler
		{
			// Token: 0x06001B1E RID: 6942 RVA: 0x0003823B File Offset: 0x0003643B
			public YanshuangCommandHandler(GameRunController gameRun)
			{
				this._gameRun = gameRun;
			}

			// Token: 0x06001B1F RID: 6943 RVA: 0x0003824A File Offset: 0x0003644A
			[RuntimeCommand("trade", "")]
			[UsedImplicitly]
			public IEnumerator Trade()
			{
				WaijieYanshuang exhibit = this._gameRun.Player.GetExhibit<WaijieYanshuang>();
				if (exhibit != null)
				{
					this._gameRun.LoseExhibit(exhibit, false, true);
				}
				else
				{
					Debug.LogError("[WaijieYanshuang] Player has no WaijieYanshuang");
				}
				yield return this._gameRun.GainExhibitRunner(Library.CreateExhibit<JingjieGanzhiyi>(), true, new VisualSourceData
				{
					SourceType = VisualSourceType.Vn
				});
				yield break;
			}

			// Token: 0x04000772 RID: 1906
			private GameRunController _gameRun;
		}
	}
}
