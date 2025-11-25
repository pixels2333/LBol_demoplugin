using System;
using JetBrains.Annotations;
using LBoL.Core.Units;
namespace LBoL.Core.Intentions
{
	[UsedImplicitly]
	public sealed class AttackIntention : Intention
	{
		public override IntentionType Type
		{
			get
			{
				return IntentionType.Attack;
			}
		}
		private string MultiDamageDescription
		{
			get
			{
				return this.LocalizeProperty("MultiDamageDescription", true, true);
			}
		}
		private string AccurateDescription
		{
			get
			{
				return this.LocalizeProperty("AccurateDescription", true, true);
			}
		}
		private string AccurateMultiDamageDescription
		{
			get
			{
				return this.LocalizeProperty("AccurateMultiDamageDescription", true, true);
			}
		}
		public DamageInfo Damage { get; internal set; }
		public int? Times { get; internal set; }
		public bool IsAccuracy { get; internal set; }
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
