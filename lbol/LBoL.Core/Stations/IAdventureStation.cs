using System;
using LBoL.Core.Adventures;
namespace LBoL.Core.Stations
{
	public interface IAdventureStation
	{
		Adventure Adventure { get; }
		void Restore(Adventure adventure);
	}
}
