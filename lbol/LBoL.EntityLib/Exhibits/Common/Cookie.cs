using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class Cookie : Exhibit
	{
		protected override void OnGain(PlayerUnit player)
		{
			base.GameRun.GainMaxHp(base.Value1, true, true);
			base.GameRun.HealToMaxHp(true, "Cookie");
		}
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
	}
}
