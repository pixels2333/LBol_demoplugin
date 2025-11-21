using System;
using LBoL.Core.Adventures;
using Yarn;

namespace LBoL.EntityLib.Adventures.FirstPlace
{
	// Token: 0x02000522 RID: 1314
	public sealed class WatatsukiPurify : Adventure
	{
		// Token: 0x0600113B RID: 4411 RVA: 0x0001FED4 File Offset: 0x0001E0D4
		protected override void InitVariables(IVariableStorage storage)
		{
			storage.SetValue("$eatPeach", 16f);
			int num = base.GameRun.Player.MaxHp / 10;
			storage.SetValue("$loseMax", (float)num);
		}
	}
}
