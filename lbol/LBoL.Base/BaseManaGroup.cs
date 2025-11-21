using System;
using Untitled.ConfigDataBuilder.Base;

namespace LBoL.Base
{
	// Token: 0x02000005 RID: 5
	[ConfigValueConverter(typeof(BaseManaGroupConverter), new string[] { })]
	public struct BaseManaGroup
	{
		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000018 RID: 24 RVA: 0x00002533 File Offset: 0x00000733
		// (set) Token: 0x06000019 RID: 25 RVA: 0x0000253B File Offset: 0x0000073B
		public ManaGroup Value { readonly get; set; }

		// Token: 0x0600001A RID: 26 RVA: 0x00002544 File Offset: 0x00000744
		public BaseManaGroup(ManaGroup mana)
		{
			this.Value = mana;
		}

		// Token: 0x0600001B RID: 27 RVA: 0x0000254D File Offset: 0x0000074D
		public static implicit operator ManaGroup(BaseManaGroup baseMana)
		{
			return baseMana.Value;
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00002556 File Offset: 0x00000756
		public static implicit operator BaseManaGroup(ManaGroup mana)
		{
			return new BaseManaGroup(mana);
		}
	}
}
