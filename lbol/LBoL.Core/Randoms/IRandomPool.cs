using System;
using System.Collections;
using System.Collections.Generic;
using LBoL.Base;
namespace LBoL.Core.Randoms
{
	public interface IRandomPool<T> : IEnumerable<RandomPoolEntry<T>>, IEnumerable
	{
		T Sample(RandomGen rng);
		T SampleOrDefault(RandomGen rng);
	}
}
