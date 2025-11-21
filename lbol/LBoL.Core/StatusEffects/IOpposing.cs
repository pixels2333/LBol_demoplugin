using System;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x020000A0 RID: 160
	public interface IOpposing<in T> where T : StatusEffect
	{
		// Token: 0x06000785 RID: 1925
		OpposeResult Oppose(T other);
	}
}
