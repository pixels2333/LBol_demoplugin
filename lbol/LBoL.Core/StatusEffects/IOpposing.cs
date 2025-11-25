using System;
namespace LBoL.Core.StatusEffects
{
	public interface IOpposing<in T> where T : StatusEffect
	{
		OpposeResult Oppose(T other);
	}
}
