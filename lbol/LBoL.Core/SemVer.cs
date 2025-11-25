using System;
namespace LBoL.Core
{
	public sealed class SemVer : IEquatable<SemVer>
	{
		public int Major { get; }
		public int Minor { get; }
		public int Patch { get; }
		public int? CommitCount { get; }
		public SemVer(int major, int minor, int patch, int? commitCount)
		{
			this.Major = major;
			this.Minor = minor;
			this.Patch = patch;
			this.CommitCount = commitCount;
		}
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
		public override string ToString()
		{
			if (this.CommitCount != null)
			{
				return string.Format("{0}.{1}.{2}.{3}", new object[] { this.Major, this.Minor, this.Patch, this.CommitCount });
			}
			return string.Format("{0}.{1}.{2}", this.Major, this.Minor, this.Patch);
		}
		public bool EqualsWithoutCount(SemVer other)
		{
			return other != null && (this == other || (this.Major == other.Major && this.Minor == other.Minor && this.Patch == other.Patch));
		}
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
		public override bool Equals(object obj)
		{
			if (this != obj)
			{
				SemVer semVer = obj as SemVer;
				return semVer != null && this.Equals(semVer);
			}
			return true;
		}
		public override int GetHashCode()
		{
			return HashCode.Combine<int, int, int, int?>(this.Major, this.Minor, this.Patch, this.CommitCount);
		}
	}
}
