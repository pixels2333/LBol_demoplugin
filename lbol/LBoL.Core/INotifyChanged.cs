using System;

namespace LBoL.Core
{
	// Token: 0x02000051 RID: 81
	public interface INotifyChanged
	{
		// Token: 0x14000006 RID: 6
		// (add) Token: 0x0600036E RID: 878
		// (remove) Token: 0x0600036F RID: 879
		event Action PropertyChanged;

		// Token: 0x06000370 RID: 880
		void NotifyChanged();
	}
}
