using System;
using System.Text;
using JetBrains.Annotations;
using LBoL.Base;

namespace LBoL.Core.GapOptions
{
	// Token: 0x02000116 RID: 278
	[UsedImplicitly]
	public sealed class UpgradeCard : GapOption
	{
		// Token: 0x17000336 RID: 822
		// (get) Token: 0x06000A08 RID: 2568 RVA: 0x0001C9D8 File Offset: 0x0001ABD8
		public override GapOptionType Type
		{
			get
			{
				return GapOptionType.UpgradeCard;
			}
		}

		// Token: 0x17000337 RID: 823
		// (get) Token: 0x06000A09 RID: 2569 RVA: 0x0001C9DB File Offset: 0x0001ABDB
		// (set) Token: 0x06000A0A RID: 2570 RVA: 0x0001C9E3 File Offset: 0x0001ABE3
		public int Price { get; internal set; }

		// Token: 0x17000338 RID: 824
		// (get) Token: 0x06000A0B RID: 2571 RVA: 0x0001C9EC File Offset: 0x0001ABEC
		private string AdditionalHealText
		{
			get
			{
				return base.LocalizeProperty("PuzzleDescription", false, true);
			}
		}

		// Token: 0x06000A0C RID: 2572 RVA: 0x0001C9FC File Offset: 0x0001ABFC
		protected override string GetBaseDescription()
		{
			StringBuilder stringBuilder = new StringBuilder(base.GetBaseDescription());
			if (this.Price > 0)
			{
				stringBuilder.AppendLine().Append(this.AdditionalHealText);
			}
			return stringBuilder.ToString();
		}
	}
}
