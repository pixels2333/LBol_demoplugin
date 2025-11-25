using System;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits
{
	public sealed class KongZhanpinhe : Exhibit
	{
		protected override void OnGain(PlayerUnit player)
		{
			base.Blackout = true;
		}
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
	}
}
