using System;
using System.Text;
using LBoL.Base;
using LBoL.Base.Extensions;
namespace LBoL.Core
{
	public struct DamageInfo : IEquatable<DamageInfo>
	{
		public float Damage { readonly get; set; }
		public float DamageBlocked { readonly get; set; }
		public float DamageShielded { readonly get; set; }
		public DamageType DamageType { readonly get; set; }
		public bool IsGrazed { readonly get; set; }
		public bool IsAccuracy { readonly get; set; }
		public int OverDamage { readonly get; set; }
		public float Amount
		{
			get
			{
				return this.Damage + this.DamageBlocked + this.DamageShielded;
			}
		}
		public bool ZeroDamage
		{
			get
			{
				return this.Damage == 0f && this.DamageBlocked == 0f && this.DamageShielded == 0f;
			}
		}
		public bool DontBreakPerfect { readonly get; set; }
		internal DamageInfo(float damage, DamageType type, bool isGrazed = false, bool isAccuracy = false, bool dontBreakPerfect = false)
		{
			this.Damage = damage;
			this.DamageBlocked = 0f;
			this.DamageShielded = 0f;
			this.DamageType = type;
			this.IsGrazed = isGrazed;
			this.IsAccuracy = isAccuracy;
			this.OverDamage = 0;
			this.DontBreakPerfect = dontBreakPerfect;
		}
		internal DamageInfo(float damage, float damageBlocked, float damageShielded, DamageType type, bool isGrazed = false, bool isAccuracy = false, bool dontBreakPerfect = false)
		{
			this.Damage = damage;
			this.DamageBlocked = damageBlocked;
			this.DamageShielded = damageShielded;
			this.DamageType = type;
			this.IsGrazed = isGrazed;
			this.IsAccuracy = isAccuracy;
			this.OverDamage = 0;
			this.DontBreakPerfect = dontBreakPerfect;
		}
		internal DamageInfo ShieldBy(int shield)
		{
			if (this.Damage <= (float)shield)
			{
				return new DamageInfo(0f, this.DamageBlocked, this.DamageShielded + this.Damage, this.DamageType, this.IsGrazed, this.IsAccuracy, false);
			}
			return new DamageInfo(this.Damage - (float)shield, this.DamageBlocked, this.DamageShielded + (float)shield, this.DamageType, this.IsGrazed, this.IsAccuracy, false);
		}
		internal DamageInfo BlockBy(int block)
		{
			if (this.Damage <= (float)block)
			{
				return new DamageInfo(0f, this.DamageBlocked + this.Damage, this.DamageShielded, this.DamageType, this.IsGrazed, this.IsAccuracy, false);
			}
			return new DamageInfo(this.Damage - (float)block, this.DamageBlocked + (float)block, this.DamageShielded, this.DamageType, this.IsGrazed, this.IsAccuracy, false);
		}
		public DamageInfo IncreaseBy(int delta)
		{
			if (!this.DamageBlocked.Approximately(0f) || !this.DamageShielded.Approximately(0f) || this.IsGrazed)
			{
				throw new InvalidOperationException("Increasing DamageInfo which is already dealt");
			}
			return new DamageInfo(this.Damage + (float)delta, this.DamageType, false, this.IsAccuracy, false);
		}
		public DamageInfo ReduceBy(int delta)
		{
			if (!this.DamageBlocked.Approximately(0f) || !this.DamageShielded.Approximately(0f) || this.IsGrazed)
			{
				throw new InvalidOperationException("Reducing DamageInfo which is already dealt");
			}
			return new DamageInfo(this.Damage - (float)delta, this.DamageType, false, this.IsAccuracy, false);
		}
		public DamageInfo ReduceActualDamageBy(int delta)
		{
			float num = Math.Max(this.Damage - (float)delta, 0f);
			float damageBlocked = this.DamageBlocked;
			float damageShielded = this.DamageShielded;
			DamageType damageType = this.DamageType;
			bool isAccuracy = this.IsAccuracy;
			return new DamageInfo(num, damageBlocked, damageShielded, damageType, this.IsGrazed, isAccuracy, false);
		}
		public DamageInfo MultiplyBy(int multiplier)
		{
			if (multiplier < 0)
			{
				throw new ArgumentException(string.Format("Multiplier ({0}) cannot be negative", multiplier));
			}
			if (!this.DamageBlocked.Approximately(0f) || !this.DamageShielded.Approximately(0f) || this.IsGrazed)
			{
				throw new InvalidOperationException("Multiplying DamageInfo which is already dealt");
			}
			return new DamageInfo(this.Damage * (float)multiplier, this.DamageType, false, this.IsAccuracy, false);
		}
		public DamageInfo MultiplyBy(float multiplier)
		{
			if (multiplier < 0f)
			{
				throw new ArgumentException(string.Format("Multiplier ({0}) cannot be negative", multiplier));
			}
			if (!this.DamageBlocked.Approximately(0f) || !this.DamageShielded.Approximately(0f) || this.IsGrazed)
			{
				throw new InvalidOperationException("Multiplying DamageInfo which is already dealt");
			}
			return new DamageInfo(this.Damage * multiplier, this.DamageType, false, this.IsAccuracy, false);
		}
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append('{').Append(this.Damage);
			if (!this.DamageBlocked.Approximately(0f))
			{
				stringBuilder.Append(" B: ").Append(this.DamageBlocked);
			}
			if (!this.DamageShielded.Approximately(0f))
			{
				stringBuilder.Append(" S: ").Append(this.DamageShielded);
			}
			if (this.IsGrazed)
			{
				stringBuilder.Append(", Grazed");
			}
			if (this.IsAccuracy)
			{
				stringBuilder.Append(", Accurary");
			}
			if (this.OverDamage > 0)
			{
				stringBuilder.Append(", Over: ").Append(this.OverDamage);
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}
		public bool Equals(DamageInfo other)
		{
			return this.Damage.Approximately(other.Damage) && this.DamageBlocked.Approximately(other.DamageBlocked) && this.DamageShielded.Approximately(other.DamageShielded) && this.DamageType == other.DamageType && this.IsGrazed == other.IsGrazed && this.IsAccuracy == other.IsAccuracy && this.OverDamage == other.OverDamage;
		}
		public override bool Equals(object obj)
		{
			if (obj is DamageInfo)
			{
				DamageInfo damageInfo = (DamageInfo)obj;
				return this.Equals(damageInfo);
			}
			return false;
		}
		public override int GetHashCode()
		{
			return HashCode.Combine<float, float, float, DamageType, bool, bool, int>(this.Damage, this.DamageBlocked, this.DamageShielded, this.DamageType, this.IsGrazed, this.IsAccuracy, this.OverDamage);
		}
		public static DamageInfo HpLose(float damage, bool dontBreakPerfect = false)
		{
			return new DamageInfo(damage, DamageType.HpLose, false, false, dontBreakPerfect);
		}
		public static DamageInfo Reaction(float damage, bool dontBreakPerfect = false)
		{
			return new DamageInfo(damage, DamageType.Reaction, false, false, dontBreakPerfect);
		}
		public static DamageInfo Attack(float damage, bool isAccuracy = false)
		{
			return new DamageInfo(damage, DamageType.Attack, false, isAccuracy, false);
		}
	}
}
