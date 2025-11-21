using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x020000FB RID: 251
	[UsedImplicitly]
	public sealed class AttackIntention : Intention
	{
		// Token: 0x170002FB RID: 763
		// (get) Token: 0x0600098F RID: 2447 RVA: 0x0001BEEB File Offset: 0x0001A0EB
		public override IntentionType Type
		{
			get
			{
				return IntentionType.Attack;
			}
		}

		// Token: 0x170002FC RID: 764
		// (get) Token: 0x06000990 RID: 2448 RVA: 0x0001BEEE File Offset: 0x0001A0EE
		private string MultiDamageDescription
		{
			get
			{
				return this.LocalizeProperty("MultiDamageDescription", true, true);
			}
		}

		// Token: 0x170002FD RID: 765
		// (get) Token: 0x06000991 RID: 2449 RVA: 0x0001BEFD File Offset: 0x0001A0FD
		private string AccurateDescription
		{
			get
			{
				return this.LocalizeProperty("AccurateDescription", true, true);
			}
		}

		// Token: 0x170002FE RID: 766
		// (get) Token: 0x06000992 RID: 2450 RVA: 0x0001BF0C File Offset: 0x0001A10C
		private string AccurateMultiDamageDescription
		{
			get
			{
				return this.LocalizeProperty("AccurateMultiDamageDescription", true, true);
			}
		}

		// Token: 0x170002FF RID: 767
		// (get) Token: 0x06000993 RID: 2451 RVA: 0x0001BF1B File Offset: 0x0001A11B
		// (set) Token: 0x06000994 RID: 2452 RVA: 0x0001BF23 File Offset: 0x0001A123
		public DamageInfo Damage { get; internal set; }

		// Token: 0x17000300 RID: 768
		// (get) Token: 0x06000995 RID: 2453 RVA: 0x0001BF2C File Offset: 0x0001A12C
		// (set) Token: 0x06000996 RID: 2454 RVA: 0x0001BF34 File Offset: 0x0001A134
		public int? Times { get; internal set; }

		// Token: 0x17000301 RID: 769
		// (get) Token: 0x06000997 RID: 2455 RVA: 0x0001BF3D File Offset: 0x0001A13D
		// (set) Token: 0x06000998 RID: 2456 RVA: 0x0001BF45 File Offset: 0x0001A145
		public bool IsAccuracy { get; internal set; }

		// Token: 0x17000302 RID: 770
		// (get) Token: 0x06000999 RID: 2457 RVA: 0x0001BF50 File Offset: 0x0001A150
		public string DamageText
		{
			get
			{
				int? times = this.Times;
				int num = 1;
				if (!((times.GetValueOrDefault() > num) & (times != null)))
				{
					return base.CalculateDamage(this.Damage).ToString();
				}
				return base.CalculateDamage(this.Damage).ToString() + "x" + this.Times.ToString();
			}
		}

		// Token: 0x17000303 RID: 771
		// (get) Token: 0x0600099A RID: 2458 RVA: 0x0001BFC4 File Offset: 0x0001A1C4
		public int TotalDamage
		{
			get
			{
				int? times = this.Times;
				int num = 1;
				if (!((times.GetValueOrDefault() > num) & (times != null)))
				{
					return base.CalculateDamage(this.Damage);
				}
				return base.CalculateDamage(this.Damage) * this.Times.Value;
			}
		}

		// Token: 0x0600099B RID: 2459 RVA: 0x0001C018 File Offset: 0x0001A218
		protected override string GetBaseDescription()
		{
			if (!this.IsAccuracy)
			{
				int? num = this.Times;
				int num2 = 1;
				if (!((num.GetValueOrDefault() > num2) & (num != null)))
				{
					return base.BaseDescription;
				}
				return this.MultiDamageDescription;
			}
			else
			{
				int? num = this.Times;
				int num2 = 1;
				if (!((num.GetValueOrDefault() > num2) & (num != null)))
				{
					return this.AccurateDescription;
				}
				return this.AccurateMultiDamageDescription;
			}
		}
	}
}
