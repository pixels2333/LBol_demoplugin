using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000167 RID: 359
	[UsedImplicitly]
	public sealed class GanzhuYao : Exhibit
	{
		// Token: 0x17000075 RID: 117
		// (get) Token: 0x060004F3 RID: 1267 RVA: 0x0000C878 File Offset: 0x0000AA78
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

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x060004F4 RID: 1268 RVA: 0x0000C899 File Offset: 0x0000AA99
		public override bool ShowCounter
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060004F5 RID: 1269 RVA: 0x0000C89C File Offset: 0x0000AA9C
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<DieEventArgs>(base.Owner.Dying, new GameEventHandler<DieEventArgs>(this.OnDying));
		}

		// Token: 0x060004F6 RID: 1270 RVA: 0x0000C8BB File Offset: 0x0000AABB
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}

		// Token: 0x060004F7 RID: 1271 RVA: 0x0000C8C4 File Offset: 0x0000AAC4
		private void OnDying(DieEventArgs args)
		{
			if (base.Counter == 0)
			{
				base.NotifyActivating();
				int num = ((double)(args.Unit.MaxHp * base.Value1) / 100.0).RoundToInt();
				base.GameRun.SetHpAndMaxHp(num, num, false);
				args.CancelBy(this);
				base.Counter = 1;
				if (base.GameRun.Battle != null)
				{
					this.React(new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Value2), default(int?), default(int?), default(int?), 0f, true));
					base.Blackout = true;
				}
			}
		}

		// Token: 0x060004F8 RID: 1272 RVA: 0x0000C978 File Offset: 0x0000AB78
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, delegate(UnitEventArgs _)
			{
				if (base.Counter == 1)
				{
					base.Blackout = true;
				}
			});
		}

		// Token: 0x060004F9 RID: 1273 RVA: 0x0000C9C4 File Offset: 0x0000ABC4
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			if (base.Counter == 1)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Value2), default(int?), default(int?), default(int?), 0f, true);
				base.Blackout = true;
			}
			yield break;
		}
	}
}
