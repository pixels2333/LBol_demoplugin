using System;
namespace LBoL.Core
{
	public interface IExhibitWeighter
	{
		float WeightFor(Type type, GameRunController gameRun);
	}
}
