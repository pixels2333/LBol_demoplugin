using System;
using JetBrains.Annotations;
using LBoL.Core;
namespace LBoL.EntityLib.JadeBoxes
{
	[UsedImplicitly]
	public sealed class FourCards : JadeBox
	{
		protected override void OnAdded()
		{
			base.GameRun.LootCardCommonDupeCount += base.Value1;
			base.GameRun.LootCardUncommonDupeCount += base.Value2;
		}
	}
}
