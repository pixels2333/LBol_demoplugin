using System;
using LBoL.Base;
using LBoL.Core.Attributes;

namespace LBoL.Core.GapOptions
{
	// Token: 0x02000114 RID: 276
	[Localizable]
	public abstract class GapOption
	{
		// Token: 0x17000328 RID: 808
		// (get) Token: 0x060009F0 RID: 2544
		public abstract GapOptionType Type { get; }

		// Token: 0x060009F1 RID: 2545 RVA: 0x0001C84C File Offset: 0x0001AA4C
		protected string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<GapOption>.LocalizeProperty(this.Type.ToString(), key, decorated, required);
		}

		// Token: 0x17000329 RID: 809
		// (get) Token: 0x060009F2 RID: 2546 RVA: 0x0001C875 File Offset: 0x0001AA75
		public string Name
		{
			get
			{
				return this.LocalizeProperty("Name", false, true);
			}
		}

		// Token: 0x1700032A RID: 810
		// (get) Token: 0x060009F3 RID: 2547 RVA: 0x0001C884 File Offset: 0x0001AA84
		protected string BaseDescription
		{
			get
			{
				return this.LocalizeProperty("Description", true, true);
			}
		}

		// Token: 0x060009F4 RID: 2548 RVA: 0x0001C893 File Offset: 0x0001AA93
		protected virtual string GetBaseDescription()
		{
			return this.BaseDescription;
		}

		// Token: 0x1700032B RID: 811
		// (get) Token: 0x060009F5 RID: 2549 RVA: 0x0001C89B File Offset: 0x0001AA9B
		public string Description
		{
			get
			{
				string baseDescription = this.GetBaseDescription();
				return ((baseDescription != null) ? baseDescription.RuntimeFormat(this) : null) ?? ("<" + base.GetType().Name + ".Description>");
			}
		}
	}
}
