using System;

namespace LBoL.Core
{
	// Token: 0x02000066 RID: 102
	[AttributeUsage(64)]
	public class RuntimeCommandAttribute : Attribute
	{
		// Token: 0x17000156 RID: 342
		// (get) Token: 0x06000456 RID: 1110 RVA: 0x0000EC35 File Offset: 0x0000CE35
		public string Name { get; }

		// Token: 0x17000157 RID: 343
		// (get) Token: 0x06000457 RID: 1111 RVA: 0x0000EC3D File Offset: 0x0000CE3D
		public string Description { get; }

		// Token: 0x06000458 RID: 1112 RVA: 0x0000EC45 File Offset: 0x0000CE45
		public RuntimeCommandAttribute(string name, string description = "")
		{
			this.Name = name;
			this.Description = description;
		}
	}
}
