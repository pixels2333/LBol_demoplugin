using System;
using JetBrains.Annotations;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class DatiangouMaifan : ShiningExhibit
	{
		protected override void OnLeaveBattle()
		{
			PlayerUnit owner = base.Owner;
			if (owner != null && owner.IsAlive)
			{
				base.NotifyActivating();
				base.GameRun.GainPower(base.Value1, true);
			}
		}
	}
}
