using System;
using LBoL.Core.Adventures;
using Yarn;
namespace LBoL.EntityLib.Adventures.Stage1
{
	public sealed class KaguyaVersusMokou : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			int num = base.GameRun.AdventureRng.NextInt(15, 18);
			storage.SetValue("$hpLose", (float)num);
			int num2 = num / 2;
			storage.SetValue("$hpLoseLow", (float)num2);
		}
	}
}
