using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.Core.Intentions
{
	// Token: 0x0200010E RID: 270
	[UsedImplicitly]
	public sealed class SpellCardIntention : Intention
	{
		// Token: 0x1700031C RID: 796
		// (get) Token: 0x060009CB RID: 2507 RVA: 0x0001C1E6 File Offset: 0x0001A3E6
		public override IntentionType Type
		{
			get
			{
				return IntentionType.SpellCard;
			}
		}

		// Token: 0x1700031D RID: 797
		// (get) Token: 0x060009CC RID: 2508 RVA: 0x0001C1EA File Offset: 0x0001A3EA
		private string DamageDescription
		{
			get
			{
				return this.LocalizeProperty("DamageDescription", true, true);
			}
		}

		// Token: 0x1700031E RID: 798
		// (get) Token: 0x060009CD RID: 2509 RVA: 0x0001C1F9 File Offset: 0x0001A3F9
		private string MultiDamageDescription
		{
			get
			{
				return this.LocalizeProperty("MultiDamageDescription", true, true);
			}
		}

		// Token: 0x1700031F RID: 799
		// (get) Token: 0x060009CE RID: 2510 RVA: 0x0001C208 File Offset: 0x0001A408
		private string AccurateDamageDescription
		{
			get
			{
				return this.LocalizeProperty("AccurateDamageDescription", true, true);
			}
		}

		// Token: 0x17000320 RID: 800
		// (get) Token: 0x060009CF RID: 2511 RVA: 0x0001C217 File Offset: 0x0001A417
		private string AccurateMultiDamageDescription
		{
			get
			{
				return this.LocalizeProperty("AccurateMultiDamageDescription", true, true);
			}
		}

		// Token: 0x17000321 RID: 801
		// (get) Token: 0x060009D0 RID: 2512 RVA: 0x0001C226 File Offset: 0x0001A426
		// (set) Token: 0x060009D1 RID: 2513 RVA: 0x0001C22E File Offset: 0x0001A42E
		public string IconName { get; internal set; }

		// Token: 0x17000322 RID: 802
		// (get) Token: 0x060009D2 RID: 2514 RVA: 0x0001C237 File Offset: 0x0001A437
		// (set) Token: 0x060009D3 RID: 2515 RVA: 0x0001C23F File Offset: 0x0001A43F
		public DamageInfo? Damage { get; internal set; }

		// Token: 0x17000323 RID: 803
		// (get) Token: 0x060009D4 RID: 2516 RVA: 0x0001C248 File Offset: 0x0001A448
		// (set) Token: 0x060009D5 RID: 2517 RVA: 0x0001C250 File Offset: 0x0001A450
		public int? Times { get; internal set; }

		// Token: 0x17000324 RID: 804
		// (get) Token: 0x060009D6 RID: 2518 RVA: 0x0001C259 File Offset: 0x0001A459
		// (set) Token: 0x060009D7 RID: 2519 RVA: 0x0001C261 File Offset: 0x0001A461
		public bool IsAccuracy { get; internal set; }

		// Token: 0x17000325 RID: 805
		// (get) Token: 0x060009D8 RID: 2520 RVA: 0x0001C26C File Offset: 0x0001A46C
		public string DamageText
		{
			get
			{
				DamageInfo? damage = this.Damage;
				if (damage == null)
				{
					return null;
				}
				DamageInfo valueOrDefault = damage.GetValueOrDefault();
				int? times = this.Times;
				int num = 1;
				if (!((times.GetValueOrDefault() > num) & (times != null)))
				{
					return base.CalculateDamage(valueOrDefault).ToString();
				}
				return base.CalculateDamage(valueOrDefault).ToString() + "x" + this.Times.ToString();
			}
		}

		// Token: 0x060009D9 RID: 2521 RVA: 0x0001C2F0 File Offset: 0x0001A4F0
		protected override string GetBaseDescription()
		{
			if (this.Damage == null)
			{
				return base.BaseDescription;
			}
			if (!this.IsAccuracy)
			{
				int? num = this.Times;
				int num2 = 1;
				if (!((num.GetValueOrDefault() > num2) & (num != null)))
				{
					return this.DamageDescription;
				}
				return this.MultiDamageDescription;
			}
			else
			{
				int? num = this.Times;
				int num2 = 1;
				if (!((num.GetValueOrDefault() > num2) & (num != null)))
				{
					return this.AccurateDamageDescription;
				}
				return this.AccurateMultiDamageDescription;
			}
		}
	}
}
