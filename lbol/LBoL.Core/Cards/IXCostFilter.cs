using System;
using LBoL.Base;

namespace LBoL.Core.Cards
{
	// Token: 0x02000132 RID: 306
	public interface IXCostFilter
	{
		// Token: 0x06000BE5 RID: 3045
		ManaGroup GetXCostFromPooled(ManaGroup pooledMana);
	}
}
