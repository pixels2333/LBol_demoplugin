using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000182 RID: 386
	[UsedImplicitly]
	public sealed class Moping : Exhibit
	{
		// Token: 0x1700007C RID: 124
		// (get) Token: 0x0600056A RID: 1386 RVA: 0x0000D35C File Offset: 0x0000B55C
		public override string OverrideIconName
		{
			get
			{
				if (base.Counter != 3)
				{
					return base.Id + base.Counter.ToString();
				}
				return base.Id;
			}
		}

		// Token: 0x0600056B RID: 1387 RVA: 0x0000D392 File Offset: 0x0000B592
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<StationEventArgs>(base.GameRun.StationEntered, delegate(StationEventArgs arg)
			{
				if (arg.Station.Type == StationType.Gap)
				{
					base.Counter = base.Config.InitialCounter.GetValueOrDefault();
					base.NotifyActivating();
				}
			});
		}

		// Token: 0x0600056C RID: 1388 RVA: 0x0000D3B1 File Offset: 0x0000B5B1
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}

		// Token: 0x0600056D RID: 1389 RVA: 0x0000D3BC File Offset: 0x0000B5BC
		protected override void OnLeaveBattle()
		{
			PlayerUnit owner = base.Owner;
			if (owner != null && owner.IsAlive && base.Counter > 0 && (base.Owner.Hp < base.Owner.MaxHp || base.Owner.Power < base.Owner.MaxPower))
			{
				int num = base.Counter - 1;
				base.Counter = num;
				base.NotifyActivating();
				base.GameRun.Heal(base.Value1, true, null);
				base.GameRun.GainPower(base.Value1, false);
			}
			base.Blackout = false;
		}
	}
}
