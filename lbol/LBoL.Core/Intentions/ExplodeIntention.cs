using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x02000103 RID: 259
	[UsedImplicitly]
	public sealed class ExplodeIntention : Intention
	{
		// Token: 0x1700030C RID: 780
		// (get) Token: 0x060009AD RID: 2477 RVA: 0x0001C0EE File Offset: 0x0001A2EE
		public override IntentionType Type
		{
			get
			{
				return IntentionType.Explode;
			}
		}

		// Token: 0x1700030D RID: 781
		// (get) Token: 0x060009AE RID: 2478 RVA: 0x0001C0F2 File Offset: 0x0001A2F2
		// (set) Token: 0x060009AF RID: 2479 RVA: 0x0001C0FA File Offset: 0x0001A2FA
		public DamageInfo Damage { get; internal set; }

		// Token: 0x1700030E RID: 782
		// (get) Token: 0x060009B0 RID: 2480 RVA: 0x0001C104 File Offset: 0x0001A304
		public string DamageText
		{
			get
			{
				return base.CalculateDamage(this.Damage).ToString();
			}
		}
	}
}
