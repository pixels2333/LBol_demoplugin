using System;
using LBoL.Core.Adventures;
namespace LBoL.Presentation.UI
{
	public abstract class UiAdventurePanel<TAdventure> : UiPanel, IAdventureHandler where TAdventure : Adventure
	{
		public TAdventure Adventure { get; private set; }
		public void EnterAdventure(Adventure adventure)
		{
			this.Adventure = (TAdventure)((object)adventure);
		}
		public void LeaveAdventure(Adventure adventure)
		{
			this.Adventure = default(TAdventure);
		}
	}
}
