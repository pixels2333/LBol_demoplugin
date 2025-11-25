using System;
using LBoL.Core.Adventures;
using Yarn;
namespace LBoL.EntityLib.Adventures.FirstPlace
{
	public sealed class WatatsukiPurify : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			storage.SetValue("$eatPeach", 16f);
			int num = base.GameRun.Player.MaxHp / 10;
			storage.SetValue("$loseMax", (float)num);
		}
	}
}
