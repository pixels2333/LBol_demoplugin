using System;
namespace LBoL.Core
{
	public struct ScryInfo
	{
		public int Count { readonly get; set; }
		public ScryInfo(int count)
		{
			this.Count = count;
		}
		public ScryInfo IncreasedBy(int delta)
		{
			return new ScryInfo(this.Count + delta);
		}
	}
}
