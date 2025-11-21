using System;
using LBoL.Core.Adventures;

namespace LBoL.Presentation.UI
{
	// Token: 0x0200002D RID: 45
	public abstract class UiAdventurePanel<TAdventure> : UiPanel, IAdventureHandler where TAdventure : Adventure
	{
		// Token: 0x1700008A RID: 138
		// (get) Token: 0x06000343 RID: 835 RVA: 0x0000E5A2 File Offset: 0x0000C7A2
		// (set) Token: 0x06000344 RID: 836 RVA: 0x0000E5AA File Offset: 0x0000C7AA
		public TAdventure Adventure { get; private set; }

		// Token: 0x06000345 RID: 837 RVA: 0x0000E5B3 File Offset: 0x0000C7B3
		public void EnterAdventure(Adventure adventure)
		{
			this.Adventure = (TAdventure)((object)adventure);
		}

		// Token: 0x06000346 RID: 838 RVA: 0x0000E5C4 File Offset: 0x0000C7C4
		public void LeaveAdventure(Adventure adventure)
		{
			this.Adventure = default(TAdventure);
		}
	}
}
