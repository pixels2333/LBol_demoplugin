using System;
using LBoL.Core.Adventures;
using Yarn;

namespace LBoL.EntityLib.Adventures.Stage1
{
	// Token: 0x0200050E RID: 1294
	public sealed class KaguyaVersusMokou : Adventure
	{
		// Token: 0x06001101 RID: 4353 RVA: 0x0001EAE0 File Offset: 0x0001CCE0
		protected override void InitVariables(IVariableStorage storage)
		{
			int num = base.GameRun.AdventureRng.NextInt(15, 18);
			storage.SetValue("$hpLose", (float)num);
			int num2 = num / 2;
			storage.SetValue("$hpLoseLow", (float)num2);
		}
	}
}
