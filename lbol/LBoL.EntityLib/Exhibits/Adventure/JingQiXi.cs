using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Adventure
{
	// Token: 0x020001C2 RID: 450
	[UsedImplicitly]
	public sealed class JingQiXi : Exhibit
	{
		// Token: 0x17000085 RID: 133
		// (get) Token: 0x0600067F RID: 1663 RVA: 0x0000EE78 File Offset: 0x0000D078
		// (set) Token: 0x06000680 RID: 1664 RVA: 0x0000EE80 File Offset: 0x0000D080
		private ManaGroup ManaLost { get; set; }

		// Token: 0x17000086 RID: 134
		// (get) Token: 0x06000681 RID: 1665 RVA: 0x0000EE89 File Offset: 0x0000D089
		[UsedImplicitly]
		public ManaGroup ManaGain
		{
			get
			{
				return this.ManaLost * 2;
			}
		}

		// Token: 0x17000087 RID: 135
		// (get) Token: 0x06000682 RID: 1666 RVA: 0x0000EE97 File Offset: 0x0000D097
		// (set) Token: 0x06000683 RID: 1667 RVA: 0x0000EE9F File Offset: 0x0000D09F
		private ManaColor LostManaColor { get; set; }

		// Token: 0x06000684 RID: 1668 RVA: 0x0000EEA8 File Offset: 0x0000D0A8
		protected override void OnEnterBattle()
		{
			this.ManaLost = ManaGroup.Empty;
			base.ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x06000685 RID: 1669 RVA: 0x0000EED4 File Offset: 0x0000D0D4
		protected override string GetBaseDescription()
		{
			if (this.ManaLost.Amount <= 0)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}

		// Token: 0x06000686 RID: 1670 RVA: 0x0000EEFF File Offset: 0x0000D0FF
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.Owner.TurnCounter % 2 == 1)
			{
				base.NotifyActivating();
				if (base.Battle.BattleMana.Amount > 0)
				{
					this.LostManaColor = base.Battle.BattleMana.EnumerateComponents().Sample(base.GameRun.BattleRng);
					this.ManaLost = ManaGroup.Single(this.LostManaColor);
					yield return new LoseManaAction(this.ManaLost);
					this.NotifyChanged();
				}
			}
			else if (this.ManaLost.Amount > 0)
			{
				base.NotifyActivating();
				yield return new GainManaAction(this.ManaGain);
				this.ManaLost = ManaGroup.Empty;
				this.NotifyChanged();
			}
			yield break;
		}
	}
}
