using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Adventure
{
	// Token: 0x020001BF RID: 447
	[UsedImplicitly]
	public sealed class IdolTshirt : Exhibit
	{
		// Token: 0x06000671 RID: 1649 RVA: 0x0000ED2C File Offset: 0x0000CF2C
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnEnded));
		}

		// Token: 0x06000672 RID: 1650 RVA: 0x0000ED78 File Offset: 0x0000CF78
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			base.NotifyActivating();
			base.GameRun.GainMaxHp(base.Counter, true, true);
			yield return new ApplyStatusEffectAction<LockedOn>(base.Battle.Player, new int?(base.Counter), default(int?), default(int?), default(int?), 0f, false);
			yield break;
		}

		// Token: 0x06000673 RID: 1651 RVA: 0x0000ED88 File Offset: 0x0000CF88
		private void OnPlayerTurnEnded(GameEventArgs args)
		{
			base.Blackout = true;
		}

		// Token: 0x06000674 RID: 1652 RVA: 0x0000ED91 File Offset: 0x0000CF91
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}

		// Token: 0x17000084 RID: 132
		// (get) Token: 0x06000675 RID: 1653 RVA: 0x0000ED9A File Offset: 0x0000CF9A
		public override string Name
		{
			get
			{
				if (base.Counter != 3)
				{
					return base.Name;
				}
				return base.ExtraDescription;
			}
		}

		// Token: 0x06000676 RID: 1654 RVA: 0x0000EDB2 File Offset: 0x0000CFB2
		[UsedImplicitly]
		public void SetToLarge()
		{
			base.Counter = 3;
		}
	}
}
