using System;
using System.Text;
using JetBrains.Annotations;
using LBoL.Base;

namespace LBoL.Core.GapOptions
{
	// Token: 0x02000115 RID: 277
	[UsedImplicitly]
	public sealed class DrinkTea : GapOption
	{
		// Token: 0x1700032C RID: 812
		// (get) Token: 0x060009F7 RID: 2551 RVA: 0x0001C8D6 File Offset: 0x0001AAD6
		public override GapOptionType Type
		{
			get
			{
				return GapOptionType.DrinkTea;
			}
		}

		// Token: 0x1700032D RID: 813
		// (get) Token: 0x060009F8 RID: 2552 RVA: 0x0001C8D9 File Offset: 0x0001AAD9
		// (set) Token: 0x060009F9 RID: 2553 RVA: 0x0001C8E1 File Offset: 0x0001AAE1
		[UsedImplicitly]
		public int Rate { get; internal set; }

		// Token: 0x1700032E RID: 814
		// (get) Token: 0x060009FA RID: 2554 RVA: 0x0001C8EA File Offset: 0x0001AAEA
		// (set) Token: 0x060009FB RID: 2555 RVA: 0x0001C8F2 File Offset: 0x0001AAF2
		public int Value { get; internal set; }

		// Token: 0x1700032F RID: 815
		// (get) Token: 0x060009FC RID: 2556 RVA: 0x0001C8FB File Offset: 0x0001AAFB
		// (set) Token: 0x060009FD RID: 2557 RVA: 0x0001C903 File Offset: 0x0001AB03
		public int AdditionalHeal { get; internal set; }

		// Token: 0x17000330 RID: 816
		// (get) Token: 0x060009FE RID: 2558 RVA: 0x0001C90C File Offset: 0x0001AB0C
		// (set) Token: 0x060009FF RID: 2559 RVA: 0x0001C914 File Offset: 0x0001AB14
		public int AdditionalPower { get; internal set; }

		// Token: 0x17000331 RID: 817
		// (get) Token: 0x06000A00 RID: 2560 RVA: 0x0001C91D File Offset: 0x0001AB1D
		// (set) Token: 0x06000A01 RID: 2561 RVA: 0x0001C925 File Offset: 0x0001AB25
		public int AdditionalCardReward { get; internal set; }

		// Token: 0x17000332 RID: 818
		// (get) Token: 0x06000A02 RID: 2562 RVA: 0x0001C92E File Offset: 0x0001AB2E
		[UsedImplicitly]
		public int CardCount
		{
			get
			{
				return 5;
			}
		}

		// Token: 0x17000333 RID: 819
		// (get) Token: 0x06000A03 RID: 2563 RVA: 0x0001C931 File Offset: 0x0001AB31
		private string AdditionalHealText
		{
			get
			{
				return base.LocalizeProperty("AdditionalHeal", false, true);
			}
		}

		// Token: 0x17000334 RID: 820
		// (get) Token: 0x06000A04 RID: 2564 RVA: 0x0001C940 File Offset: 0x0001AB40
		private string AdditionalPowerText
		{
			get
			{
				return base.LocalizeProperty("AdditionalPower", false, true);
			}
		}

		// Token: 0x17000335 RID: 821
		// (get) Token: 0x06000A05 RID: 2565 RVA: 0x0001C94F File Offset: 0x0001AB4F
		private string AdditionalCardRewardText
		{
			get
			{
				return base.LocalizeProperty("AdditionalCardReward", false, true);
			}
		}

		// Token: 0x06000A06 RID: 2566 RVA: 0x0001C960 File Offset: 0x0001AB60
		protected override string GetBaseDescription()
		{
			StringBuilder stringBuilder = new StringBuilder(base.GetBaseDescription());
			if (this.AdditionalHeal > 0)
			{
				stringBuilder.AppendLine().Append(this.AdditionalHealText);
			}
			if (this.AdditionalPower > 0)
			{
				stringBuilder.AppendLine().Append(this.AdditionalPowerText);
			}
			if (this.AdditionalCardReward > 0)
			{
				stringBuilder.AppendLine().Append(this.AdditionalCardRewardText);
			}
			return stringBuilder.ToString();
		}
	}
}
