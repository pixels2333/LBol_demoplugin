using System;
namespace LBoL.Core
{
	public class EntityName : IFormattable
	{
		public EntityName(string qualifiedId, string fallback)
		{
			this._qualifiedId = qualifiedId;
			this._fallback = fallback;
		}
		public string ToString(string format)
		{
			return EntityNameTable.TryGet(this._qualifiedId, format) ?? this._fallback;
		}
		public override string ToString()
		{
			return this.ToString(string.Empty, null);
		}
		public string ToString(string format, IFormatProvider formatProvider)
		{
			return this.ToString(format);
		}
		private readonly string _qualifiedId;
		private readonly string _fallback;
	}
}
