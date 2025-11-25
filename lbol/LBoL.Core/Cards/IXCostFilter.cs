using System;
using LBoL.Base;
namespace LBoL.Core.Cards
{
	public interface IXCostFilter
	{
		ManaGroup GetXCostFromPooled(ManaGroup pooledMana);
	}
}
