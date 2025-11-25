using System;
using JetBrains.Annotations;
using LBoL.Core.Units;
namespace LBoL.Core.Intentions
{
	[UsedImplicitly]
	public sealed class SpellCardIntention : Intention
	{
		public override IntentionType Type
		{
			get
			{
				return IntentionType.SpellCard;
			}
		}
		private string DamageDescription
		{
			get
			{
				return this.LocalizeProperty("DamageDescription", true, true);
			}
		}
		private string MultiDamageDescription
		{
			get
			{
				return this.LocalizeProperty("MultiDamageDescription", true, true);
			}
		}
		private string AccurateDamageDescription
		{
			get
			{
				return this.LocalizeProperty("AccurateDamageDescription", true, true);
			}
		}
		private string AccurateMultiDamageDescription
		{
			get
			{
				return this.LocalizeProperty("AccurateMultiDamageDescription", true, true);
			}
		}
		public string IconName { get; internal set; }
		public DamageInfo? Damage { get; internal set; }
		public int? Times { get; internal set; }
		public bool IsAccuracy { get; internal set; }
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
