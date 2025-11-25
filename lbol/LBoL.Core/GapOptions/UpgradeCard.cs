using System;
using System.Text;
using JetBrains.Annotations;
using LBoL.Base;
namespace LBoL.Core.GapOptions
{
	[UsedImplicitly]
	public sealed class UpgradeCard : GapOption
	{
		public override GapOptionType Type
		{
			get
			{
				return GapOptionType.UpgradeCard;
			}
		}
		public int Price { get; internal set; }
		private string AdditionalHealText
		{
			get
			{
				return base.LocalizeProperty("PuzzleDescription", false, true);
			}
		}
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
