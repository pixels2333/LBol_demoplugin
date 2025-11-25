using System;
using LBoL.Core.Adventures;
namespace LBoL.Presentation.UI
{
	public interface IAdventureHandler
	{
		void EnterAdventure(Adventure adventure);
		void LeaveAdventure(Adventure adventure);
	}
}
