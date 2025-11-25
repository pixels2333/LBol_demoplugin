using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class PortalGun : Exhibit
	{
		private float Multiplier
		{
			get
			{
				return (float)(100 - base.Value1) / 100f;
			}
		}
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.ShopPriceMultiplier *= this.Multiplier;
			GameRunController gameRun = base.GameRun;
			int num = gameRun.ShopResupplyFlag + 1;
			gameRun.ShopResupplyFlag = num;
		}
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.ShopPriceMultiplier /= this.Multiplier;
			GameRunController gameRun = base.GameRun;
			int num = gameRun.ShopResupplyFlag - 1;
			gameRun.ShopResupplyFlag = num;
		}
	}
}
