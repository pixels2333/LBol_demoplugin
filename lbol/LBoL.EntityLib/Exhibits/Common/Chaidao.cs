using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class Chaidao : Exhibit
	{
		protected override void OnGain(PlayerUnit player)
		{
			base.GameRun.UpgradeRandomCards(base.Value1, new CardType?(CardType.Attack));
		}
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
	}
}
