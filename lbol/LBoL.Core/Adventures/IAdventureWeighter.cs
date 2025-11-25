using System;
namespace LBoL.Core.Adventures
{
	public interface IAdventureWeighter
	{
		float WeightFor(Type type, GameRunController gameRun);
	}
}
