using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class Moping : Exhibit
	{
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
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
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
