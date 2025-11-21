using System;

namespace LBoL.Core.Dialogs
{
	// Token: 0x0200011F RID: 287
	public sealed class DialogFunctionAttribute : Attribute
	{
		// Token: 0x17000342 RID: 834
		// (get) Token: 0x06000A30 RID: 2608 RVA: 0x0001CCD7 File Offset: 0x0001AED7
		public string Name { get; }

		// Token: 0x06000A31 RID: 2609 RVA: 0x0001CCDF File Offset: 0x0001AEDF
		public DialogFunctionAttribute(string name)
		{
			this.Name = name;
		}
	}
}
