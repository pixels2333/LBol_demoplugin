using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.EntityLib.Exhibits.Adventure;
using Yarn;

namespace LBoL.EntityLib.Adventures.FirstPlace
{
	// Token: 0x0200051D RID: 1309
	public sealed class DoremyPortal : Adventure
	{
		// Token: 0x0600112D RID: 4397 RVA: 0x0001FBCC File Offset: 0x0001DDCC
		protected override void InitVariables(IVariableStorage storage)
		{
			Exhibit eliteEnemyExhibit = base.Stage.GetEliteEnemyExhibit();
			storage.SetValue("$randomExhibit", eliteEnemyExhibit.Id);
			storage.SetValue("$isSentinel", eliteEnemyExhibit.Config.IsSentinel);
			storage.SetValue("$hasGanzhiyi", base.GameRun.Player.HasExhibit<JingjieGanzhiyi>());
		}

		// Token: 0x0600112E RID: 4398 RVA: 0x0001FC27 File Offset: 0x0001DE27
		[RuntimeCommand("teleportBoss", "")]
		[UsedImplicitly]
		public void TeleportBoss()
		{
			base.GameRun.AddMapModeOverrider(new DoremyPortal.Overrider(base.GameRun));
		}

		// Token: 0x02000A77 RID: 2679
		private class Overrider : IMapModeOverrider
		{
			// Token: 0x06003776 RID: 14198 RVA: 0x00086A6B File Offset: 0x00084C6B
			public Overrider(GameRunController gameRun)
			{
				this._gameRun = gameRun;
			}

			// Token: 0x17000ACC RID: 2764
			// (get) Token: 0x06003777 RID: 14199 RVA: 0x00086A7A File Offset: 0x00084C7A
			public GameRunMapMode? MapMode
			{
				get
				{
					return new GameRunMapMode?(GameRunMapMode.TeleportBoss);
				}
			}

			// Token: 0x06003778 RID: 14200 RVA: 0x00086A82 File Offset: 0x00084C82
			public void OnEnteredWithMode()
			{
				this._gameRun.RemoveMapModeOverrider(this);
			}

			// Token: 0x0400198E RID: 6542
			private readonly GameRunController _gameRun;
		}
	}
}
