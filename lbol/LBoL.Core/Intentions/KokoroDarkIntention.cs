using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x02000107 RID: 263
	[UsedImplicitly]
	public sealed class KokoroDarkIntention : Intention
	{
		// Token: 0x17000312 RID: 786
		// (get) Token: 0x060009B8 RID: 2488 RVA: 0x0001C14F File Offset: 0x0001A34F
		public override IntentionType Type
		{
			get
			{
				return IntentionType.KokoroDark;
			}
		}

		// Token: 0x17000313 RID: 787
		// (get) Token: 0x060009B9 RID: 2489 RVA: 0x0001C153 File Offset: 0x0001A353
		// (set) Token: 0x060009BA RID: 2490 RVA: 0x0001C15B File Offset: 0x0001A35B
		public DamageInfo Damage { get; internal set; }

		// Token: 0x17000314 RID: 788
		// (get) Token: 0x060009BB RID: 2491 RVA: 0x0001C164 File Offset: 0x0001A364
		// (set) Token: 0x060009BC RID: 2492 RVA: 0x0001C16C File Offset: 0x0001A36C
		public int Count { get; internal set; }

		// Token: 0x17000315 RID: 789
		// (get) Token: 0x060009BD RID: 2493 RVA: 0x0001C178 File Offset: 0x0001A378
		public string DamageText
		{
			get
			{
				return base.CalculateDamage(this.Damage).ToString();
			}
		}
	}
}
