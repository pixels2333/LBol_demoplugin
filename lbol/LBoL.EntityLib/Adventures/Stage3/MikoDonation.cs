using System;
using LBoL.Core;
using LBoL.Core.Adventures;
using Yarn;

namespace LBoL.EntityLib.Adventures.Stage3
{
	// Token: 0x02000506 RID: 1286
	[AdventureInfo(WeighterType = typeof(MikoDonation.MikoDonationWeighter))]
	public sealed class MikoDonation : Adventure
	{
		// Token: 0x060010EB RID: 4331 RVA: 0x0001E4DD File Offset: 0x0001C6DD
		protected override void InitVariables(IVariableStorage storage)
		{
			storage.SetValue("$money", 200f);
		}

		// Token: 0x04000127 RID: 295
		private const int Money = 200;

		// Token: 0x02000A60 RID: 2656
		private class MikoDonationWeighter : IAdventureWeighter
		{
			// Token: 0x0600372F RID: 14127 RVA: 0x000860DC File Offset: 0x000842DC
			public float WeightFor(Type type, GameRunController gameRun)
			{
				if (gameRun.Money < 200)
				{
					return 0f;
				}
				return 1f;
			}
		}
	}
}
