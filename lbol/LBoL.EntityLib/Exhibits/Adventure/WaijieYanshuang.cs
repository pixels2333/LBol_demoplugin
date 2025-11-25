using System;
using System.Collections;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.Core.Units;
using UnityEngine;
namespace LBoL.EntityLib.Exhibits.Adventure
{
	[UsedImplicitly]
	public sealed class WaijieYanshuang : Exhibit
	{
		public override bool ShowCounter
		{
			get
			{
				return false;
			}
		}
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
		protected override void OnLeaveBattle()
		{
			PlayerUnit owner = base.Owner;
			if (owner != null && owner.IsAlive)
			{
				base.NotifyActivating();
				base.GameRun.Heal(base.Value1, true, null);
			}
		}
		private class YanshuangCommandHandler
		{
			public YanshuangCommandHandler(GameRunController gameRun)
			{
				this._gameRun = gameRun;
			}
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
			private GameRunController _gameRun;
		}
	}
}
