using System;
using System.Collections;
using System.Collections.Generic;
using LBoL.Base;

namespace LBoL.Core.Randoms
{
	// Token: 0x020000EE RID: 238
	public interface IRandomPool<T> : IEnumerable<RandomPoolEntry<T>>, IEnumerable
	{
		// Token: 0x0600092F RID: 2351
		T Sample(RandomGen rng);

		// Token: 0x06000930 RID: 2352
		T SampleOrDefault(RandomGen rng);
	}
}
