using System;

namespace LBoL.Core
{
	// Token: 0x02000048 RID: 72
	public interface IDisplayWord : IEquatable<IDisplayWord>
	{
		// Token: 0x1700012F RID: 303
		// (get) Token: 0x06000348 RID: 840
		string Name { get; }

		// Token: 0x17000130 RID: 304
		// (get) Token: 0x06000349 RID: 841
		string Description { get; }

		// Token: 0x17000131 RID: 305
		// (get) Token: 0x0600034A RID: 842
		bool IsVerbose { get; }
	}
}
