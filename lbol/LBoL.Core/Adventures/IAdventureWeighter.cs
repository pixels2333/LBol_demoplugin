using System;

namespace LBoL.Core.Adventures
{
	// Token: 0x020001BD RID: 445
	public interface IAdventureWeighter
	{
		// Token: 0x06000FDE RID: 4062
		float WeightFor(Type type, GameRunController gameRun);
	}
}
