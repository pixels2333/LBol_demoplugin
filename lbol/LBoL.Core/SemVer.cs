using System;

namespace LBoL.Core
{
	// Token: 0x02000078 RID: 120
	public sealed class SemVer : IEquatable<SemVer>
	{
		// Token: 0x17000194 RID: 404
		// (get) Token: 0x06000546 RID: 1350 RVA: 0x00011ABF File Offset: 0x0000FCBF
		public int Major { get; }

		// Token: 0x17000195 RID: 405
		// (get) Token: 0x06000547 RID: 1351 RVA: 0x00011AC7 File Offset: 0x0000FCC7
		public int Minor { get; }

		// Token: 0x17000196 RID: 406
		// (get) Token: 0x06000548 RID: 1352 RVA: 0x00011ACF File Offset: 0x0000FCCF
		public int Patch { get; }

		// Token: 0x17000197 RID: 407
		// (get) Token: 0x06000549 RID: 1353 RVA: 0x00011AD7 File Offset: 0x0000FCD7
		public int? CommitCount { get; }

		// Token: 0x0600054A RID: 1354 RVA: 0x00011AE0 File Offset: 0x0000FCE0
		public SemVer(int major, int minor, int patch, int? commitCount)
		{
			this.Major = major;
			this.Minor = minor;
			this.Patch = patch;
			this.CommitCount = commitCount;
		}

		// Token: 0x0600054B RID: 1355 RVA: 0x00011B18 File Offset: 0x0000FD18
		public static bool TryParse(string str, out SemVer semVer)
		{
			semVer = null;
			if (str == null)
			{
				return false;
			}
			ReadOnlySpan<char> readOnlySpan = MemoryExtensions.Trim(MemoryExtensions.AsSpan(str));
			int num = MemoryExtensions.IndexOf<char>(readOnlySpan, '.');
			if (num < 0)
			{
				return false;
			}
			ReadOnlySpan<char> readOnlySpan2 = readOnlySpan;
			int num2;
			if (!int.TryParse(readOnlySpan2.Slice(0, num), 0, null, ref num2))
			{
				return false;
			}
			readOnlySpan2 = readOnlySpan;
			int num3 = num + 1;
			readOnlySpan = readOnlySpan2.Slice(num3, readOnlySpan2.Length - num3);
			int num4 = MemoryExtensions.IndexOf<char>(readOnlySpan, '.');
			if (num4 < 0)
			{
				return false;
			}
			readOnlySpan2 = readOnlySpan;
			int num5;
			if (!int.TryParse(readOnlySpan2.Slice(0, num4), 0, null, ref num5))
			{
				return false;
			}
			readOnlySpan2 = readOnlySpan;
			num3 = num4 + 1;
			readOnlySpan = readOnlySpan2.Slice(num3, readOnlySpan2.Length - num3);
			ReadOnlySpan<char> readOnlySpan3 = null;
			int num6 = MemoryExtensions.IndexOf<char>(readOnlySpan, '.');
			if (num6 >= 0)
			{
				readOnlySpan2 = readOnlySpan;
				num3 = num6 + 1;
				readOnlySpan3 = readOnlySpan2.Slice(num3, readOnlySpan2.Length - num3);
				readOnlySpan2 = readOnlySpan;
				readOnlySpan = readOnlySpan2.Slice(0, num6);
			}
			int num7;
			if (!int.TryParse(readOnlySpan, 0, null, ref num7))
			{
				return false;
			}
			int? num8 = default(int?);
			if (readOnlySpan3 != null)
			{
				int num9;
				if (!int.TryParse(readOnlySpan3, 0, null, ref num9))
				{
					return false;
				}
				num8 = new int?(num9);
			}
			semVer = new SemVer(num2, num5, num7, num8);
			return true;
		}

		// Token: 0x0600054C RID: 1356 RVA: 0x00011C50 File Offset: 0x0000FE50
		public override string ToString()
		{
			if (this.CommitCount != null)
			{
				return string.Format("{0}.{1}.{2}.{3}", new object[] { this.Major, this.Minor, this.Patch, this.CommitCount });
			}
			return string.Format("{0}.{1}.{2}", this.Major, this.Minor, this.Patch);
		}

		// Token: 0x0600054D RID: 1357 RVA: 0x00011CE1 File Offset: 0x0000FEE1
		public bool EqualsWithoutCount(SemVer other)
		{
			return other != null && (this == other || (this.Major == other.Major && this.Minor == other.Minor && this.Patch == other.Patch));
		}

		// Token: 0x0600054E RID: 1358 RVA: 0x00011D1C File Offset: 0x0000FF1C
		public bool Equals(SemVer other)
		{
			if (other == null)
			{
				return false;
			}
			if (this == other)
			{
				return true;
			}
			if (this.Major == other.Major && this.Minor == other.Minor && this.Patch == other.Patch)
			{
				int? commitCount = this.CommitCount;
				int? commitCount2 = other.CommitCount;
				return (commitCount.GetValueOrDefault() == commitCount2.GetValueOrDefault()) & (commitCount != null == (commitCount2 != null));
			}
			return false;
		}

		// Token: 0x0600054F RID: 1359 RVA: 0x00011D90 File Offset: 0x0000FF90
		public override bool Equals(object obj)
		{
			if (this != obj)
			{
				SemVer semVer = obj as SemVer;
				return semVer != null && this.Equals(semVer);
			}
			return true;
		}

		// Token: 0x06000550 RID: 1360 RVA: 0x00011DB6 File Offset: 0x0000FFB6
		public override int GetHashCode()
		{
			return HashCode.Combine<int, int, int, int?>(this.Major, this.Minor, this.Patch, this.CommitCount);
		}
	}
}
