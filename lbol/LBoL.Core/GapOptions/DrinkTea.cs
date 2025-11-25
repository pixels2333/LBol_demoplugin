using System;
using System.Text;
using JetBrains.Annotations;
using LBoL.Base;
namespace LBoL.Core.GapOptions
{
	[UsedImplicitly]
	public sealed class DrinkTea : GapOption
	{
		public override GapOptionType Type
		{
			get
			{
				return GapOptionType.DrinkTea;
			}
		}
		[UsedImplicitly]
		public int Rate { get; internal set; }
		public int Value { get; internal set; }
		public int AdditionalHeal { get; internal set; }
		public int AdditionalPower { get; internal set; }
		public int AdditionalCardReward { get; internal set; }
		[UsedImplicitly]
		public int CardCount
		{
			get
			{
				return 5;
			}
		}
		private string AdditionalHealText
		{
			get
			{
				return base.LocalizeProperty("AdditionalHeal", false, true);
			}
		}
		private string AdditionalPowerText
		{
			get
			{
				return base.LocalizeProperty("AdditionalPower", false, true);
			}
		}
		private string AdditionalCardRewardText
		{
			get
			{
				return base.LocalizeProperty("AdditionalCardReward", false, true);
			}
		}
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
