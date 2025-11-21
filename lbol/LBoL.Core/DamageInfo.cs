using System;
using System.Text;
using LBoL.Base;
using LBoL.Base.Extensions;

namespace LBoL.Core
{
	// Token: 0x02000009 RID: 9
	public struct DamageInfo : IEquatable<DamageInfo>
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000016 RID: 22 RVA: 0x000022FD File Offset: 0x000004FD
		// (set) Token: 0x06000017 RID: 23 RVA: 0x00002305 File Offset: 0x00000505
		public float Damage { readonly get; set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000018 RID: 24 RVA: 0x0000230E File Offset: 0x0000050E
		// (set) Token: 0x06000019 RID: 25 RVA: 0x00002316 File Offset: 0x00000516
		public float DamageBlocked { readonly get; set; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600001A RID: 26 RVA: 0x0000231F File Offset: 0x0000051F
		// (set) Token: 0x0600001B RID: 27 RVA: 0x00002327 File Offset: 0x00000527
		public float DamageShielded { readonly get; set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600001C RID: 28 RVA: 0x00002330 File Offset: 0x00000530
		// (set) Token: 0x0600001D RID: 29 RVA: 0x00002338 File Offset: 0x00000538
		public DamageType DamageType { readonly get; set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600001E RID: 30 RVA: 0x00002341 File Offset: 0x00000541
		// (set) Token: 0x0600001F RID: 31 RVA: 0x00002349 File Offset: 0x00000549
		public bool IsGrazed { readonly get; set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000020 RID: 32 RVA: 0x00002352 File Offset: 0x00000552
		// (set) Token: 0x06000021 RID: 33 RVA: 0x0000235A File Offset: 0x0000055A
		public bool IsAccuracy { readonly get; set; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000022 RID: 34 RVA: 0x00002363 File Offset: 0x00000563
		// (set) Token: 0x06000023 RID: 35 RVA: 0x0000236B File Offset: 0x0000056B
		public int OverDamage { readonly get; set; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000024 RID: 36 RVA: 0x00002374 File Offset: 0x00000574
		public float Amount
		{
			get
			{
				return this.Damage + this.DamageBlocked + this.DamageShielded;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000025 RID: 37 RVA: 0x0000238A File Offset: 0x0000058A
		public bool ZeroDamage
		{
			get
			{
				return this.Damage == 0f && this.DamageBlocked == 0f && this.DamageShielded == 0f;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000026 RID: 38 RVA: 0x000023B5 File Offset: 0x000005B5
		// (set) Token: 0x06000027 RID: 39 RVA: 0x000023BD File Offset: 0x000005BD
		public bool DontBreakPerfect { readonly get; set; }

		// Token: 0x06000028 RID: 40 RVA: 0x000023C8 File Offset: 0x000005C8
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

		// Token: 0x06000029 RID: 41 RVA: 0x00002417 File Offset: 0x00000617
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

		// Token: 0x0600002A RID: 42 RVA: 0x00002458 File Offset: 0x00000658
		internal DamageInfo ShieldBy(int shield)
		{
			if (this.Damage <= (float)shield)
			{
				return new DamageInfo(0f, this.DamageBlocked, this.DamageShielded + this.Damage, this.DamageType, this.IsGrazed, this.IsAccuracy, false);
			}
			return new DamageInfo(this.Damage - (float)shield, this.DamageBlocked, this.DamageShielded + (float)shield, this.DamageType, this.IsGrazed, this.IsAccuracy, false);
		}

		// Token: 0x0600002B RID: 43 RVA: 0x000024D0 File Offset: 0x000006D0
		internal DamageInfo BlockBy(int block)
		{
			if (this.Damage <= (float)block)
			{
				return new DamageInfo(0f, this.DamageBlocked + this.Damage, this.DamageShielded, this.DamageType, this.IsGrazed, this.IsAccuracy, false);
			}
			return new DamageInfo(this.Damage - (float)block, this.DamageBlocked + (float)block, this.DamageShielded, this.DamageType, this.IsGrazed, this.IsAccuracy, false);
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002548 File Offset: 0x00000748
		public DamageInfo IncreaseBy(int delta)
		{
			if (!this.DamageBlocked.Approximately(0f) || !this.DamageShielded.Approximately(0f) || this.IsGrazed)
			{
				throw new InvalidOperationException("Increasing DamageInfo which is already dealt");
			}
			return new DamageInfo(this.Damage + (float)delta, this.DamageType, false, this.IsAccuracy, false);
		}

		// Token: 0x0600002D RID: 45 RVA: 0x000025A8 File Offset: 0x000007A8
		public DamageInfo ReduceBy(int delta)
		{
			if (!this.DamageBlocked.Approximately(0f) || !this.DamageShielded.Approximately(0f) || this.IsGrazed)
			{
				throw new InvalidOperationException("Reducing DamageInfo which is already dealt");
			}
			return new DamageInfo(this.Damage - (float)delta, this.DamageType, false, this.IsAccuracy, false);
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00002608 File Offset: 0x00000808
		public DamageInfo ReduceActualDamageBy(int delta)
		{
			float num = Math.Max(this.Damage - (float)delta, 0f);
			float damageBlocked = this.DamageBlocked;
			float damageShielded = this.DamageShielded;
			DamageType damageType = this.DamageType;
			bool isAccuracy = this.IsAccuracy;
			return new DamageInfo(num, damageBlocked, damageShielded, damageType, this.IsGrazed, isAccuracy, false);
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00002650 File Offset: 0x00000850
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

		// Token: 0x06000030 RID: 48 RVA: 0x000026CC File Offset: 0x000008CC
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

		// Token: 0x06000031 RID: 49 RVA: 0x0000274C File Offset: 0x0000094C
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

		// Token: 0x06000032 RID: 50 RVA: 0x0000281C File Offset: 0x00000A1C
		public bool Equals(DamageInfo other)
		{
			return this.Damage.Approximately(other.Damage) && this.DamageBlocked.Approximately(other.DamageBlocked) && this.DamageShielded.Approximately(other.DamageShielded) && this.DamageType == other.DamageType && this.IsGrazed == other.IsGrazed && this.IsAccuracy == other.IsAccuracy && this.OverDamage == other.OverDamage;
		}

		// Token: 0x06000033 RID: 51 RVA: 0x000028A4 File Offset: 0x00000AA4
		public override bool Equals(object obj)
		{
			if (obj is DamageInfo)
			{
				DamageInfo damageInfo = (DamageInfo)obj;
				return this.Equals(damageInfo);
			}
			return false;
		}

		// Token: 0x06000034 RID: 52 RVA: 0x000028C9 File Offset: 0x00000AC9
		public override int GetHashCode()
		{
			return HashCode.Combine<float, float, float, DamageType, bool, bool, int>(this.Damage, this.DamageBlocked, this.DamageShielded, this.DamageType, this.IsGrazed, this.IsAccuracy, this.OverDamage);
		}

		// Token: 0x06000035 RID: 53 RVA: 0x000028FA File Offset: 0x00000AFA
		public static DamageInfo HpLose(float damage, bool dontBreakPerfect = false)
		{
			return new DamageInfo(damage, DamageType.HpLose, false, false, dontBreakPerfect);
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00002906 File Offset: 0x00000B06
		public static DamageInfo Reaction(float damage, bool dontBreakPerfect = false)
		{
			return new DamageInfo(damage, DamageType.Reaction, false, false, dontBreakPerfect);
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00002912 File Offset: 0x00000B12
		public static DamageInfo Attack(float damage, bool isAccuracy = false)
		{
			return new DamageInfo(damage, DamageType.Attack, false, isAccuracy, false);
		}
	}
}
