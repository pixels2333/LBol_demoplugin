using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.EntityLib.Exhibits.Adventure;
using Yarn;
namespace LBoL.EntityLib.Adventures.FirstPlace
{
	public sealed class DoremyPortal : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			Exhibit eliteEnemyExhibit = base.Stage.GetEliteEnemyExhibit();
			storage.SetValue("$randomExhibit", eliteEnemyExhibit.Id);
			storage.SetValue("$isSentinel", eliteEnemyExhibit.Config.IsSentinel);
			storage.SetValue("$hasGanzhiyi", base.GameRun.Player.HasExhibit<JingjieGanzhiyi>());
		}
		[RuntimeCommand("teleportBoss", "")]
		[UsedImplicitly]
		public void TeleportBoss()
		{
			base.GameRun.AddMapModeOverrider(new DoremyPortal.Overrider(base.GameRun));
		}
		private class Overrider : IMapModeOverrider
		{
			public Overrider(GameRunController gameRun)
			{
				this._gameRun = gameRun;
			}
			public GameRunMapMode? MapMode
			{
				get
				{
					return new GameRunMapMode?(GameRunMapMode.TeleportBoss);
				}
			}
			public void OnEnteredWithMode()
			{
				this._gameRun.RemoveMapModeOverrider(this);
			}
			private readonly GameRunController _gameRun;
		}
	}
}
