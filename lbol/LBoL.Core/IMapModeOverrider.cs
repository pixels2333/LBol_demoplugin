using System;

namespace LBoL.Core
{
	// Token: 0x0200004F RID: 79
	public interface IMapModeOverrider
	{
		// Token: 0x17000135 RID: 309
		// (get) Token: 0x0600036B RID: 875
		GameRunMapMode? MapMode { get; }

		// Token: 0x0600036C RID: 876
		void OnEnteredWithMode();
	}
}
