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
	// Token: 0x020001A0 RID: 416
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3, ExpireStationLevel = 9)]
	public sealed class TiangouYuyi : Exhibit, IMapModeOverrider
	{
		// Token: 0x060005EE RID: 1518 RVA: 0x0000DF4C File Offset: 0x0000C14C
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.AddMapModeOverrider(this);
		}

		// Token: 0x060005EF RID: 1519 RVA: 0x0000DF5A File Offset: 0x0000C15A
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.RemoveMapModeOverrider(this);
		}

		// Token: 0x060005F0 RID: 1520 RVA: 0x0000DF68 File Offset: 0x0000C168
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x060005F1 RID: 1521 RVA: 0x0000DF8C File Offset: 0x0000C18C
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}

		// Token: 0x060005F2 RID: 1522 RVA: 0x0000DF95 File Offset: 0x0000C195
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

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x060005F3 RID: 1523 RVA: 0x0000DFA8 File Offset: 0x0000C1A8
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

		// Token: 0x060005F4 RID: 1524 RVA: 0x0000DFD0 File Offset: 0x0000C1D0
		public void OnEnteredWithMode()
		{
			int num = base.Counter - 1;
			base.Counter = num;
			base.NotifyActivating();
			this.NotifyChanged();
		}
	}
}
