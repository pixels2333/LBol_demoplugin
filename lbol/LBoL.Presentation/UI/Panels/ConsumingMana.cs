using System;
using LBoL.Base;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x02000090 RID: 144
	public class ConsumingMana
	{
		// Token: 0x17000144 RID: 324
		// (get) Token: 0x06000796 RID: 1942 RVA: 0x00023A9B File Offset: 0x00021C9B
		public ManaGroup Unpooled { get; }

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x06000797 RID: 1943 RVA: 0x00023AA3 File Offset: 0x00021CA3
		public ManaGroup Pooled { get; }

		// Token: 0x17000146 RID: 326
		// (get) Token: 0x06000798 RID: 1944 RVA: 0x00023AAB File Offset: 0x00021CAB
		public ManaGroup TotalMana
		{
			get
			{
				return this.Unpooled + this.Pooled;
			}
		}

		// Token: 0x06000799 RID: 1945 RVA: 0x00023ABE File Offset: 0x00021CBE
		public ConsumingMana(ManaGroup unpooled, ManaGroup pooled)
		{
			this.Unpooled = unpooled;
			this.Pooled = pooled;
		}

		// Token: 0x0600079A RID: 1946 RVA: 0x00023AD4 File Offset: 0x00021CD4
		public override string ToString()
		{
			return string.Format("{0}+{1}", this.Unpooled, this.Pooled);
		}
	}
}
