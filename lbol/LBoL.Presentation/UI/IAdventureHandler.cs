using System;
using LBoL.Core.Adventures;

namespace LBoL.Presentation.UI
{
	// Token: 0x0200001E RID: 30
	public interface IAdventureHandler
	{
		// Token: 0x060002FC RID: 764
		void EnterAdventure(Adventure adventure);

		// Token: 0x060002FD RID: 765
		void LeaveAdventure(Adventure adventure);
	}
}
