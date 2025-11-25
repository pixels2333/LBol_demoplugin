using System;
using JetBrains.Annotations;
using LBoL.Core;
namespace LBoL.EntityLib.JadeBoxes
{
	[UsedImplicitly]
	public sealed class AllCharacterCards : JadeBox
	{
		protected override void OnAdded()
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.AllCharacterCardsFlag + 1;
			gameRun.AllCharacterCardsFlag = num;
		}
	}
}
