using System;
using LBoL.Core.Adventures;

namespace LBoL.Core.Stations
{
	// Token: 0x020000C2 RID: 194
	public interface IAdventureStation
	{
		// Token: 0x170002B0 RID: 688
		// (get) Token: 0x06000868 RID: 2152
		Adventure Adventure { get; }

		// Token: 0x06000869 RID: 2153
		void Restore(Adventure adventure);
	}
}
