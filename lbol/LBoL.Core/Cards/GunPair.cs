using System;

namespace LBoL.Core.Cards
{
	// Token: 0x0200012F RID: 303
	public class GunPair
	{
		// Token: 0x06000BD8 RID: 3032 RVA: 0x0002129A File Offset: 0x0001F49A
		public GunPair(string gunName, GunType gunType)
		{
			this.GunName = gunName;
			this.GunType = gunType;
		}

		// Token: 0x170003F9 RID: 1017
		// (get) Token: 0x06000BD9 RID: 3033 RVA: 0x000212B0 File Offset: 0x0001F4B0
		public string GunName { get; }

		// Token: 0x170003FA RID: 1018
		// (get) Token: 0x06000BDA RID: 3034 RVA: 0x000212B8 File Offset: 0x0001F4B8
		// (set) Token: 0x06000BDB RID: 3035 RVA: 0x000212C0 File Offset: 0x0001F4C0
		public GunType GunType { get; set; }
	}
}
