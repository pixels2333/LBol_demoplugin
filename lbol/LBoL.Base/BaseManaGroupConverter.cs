using System;
using System.IO;
using Untitled.ConfigDataBuilder.Base;
namespace LBoL.Base
{
	public sealed class BaseManaGroupConverter : IConfigValueConverter<BaseManaGroup>
	{
		public BaseManaGroup Parse(string value)
		{
			return ManaGroup.Parse(value);
		}
		public void WriteTo(BinaryWriter writer, BaseManaGroup value)
		{
			value.Value.WriteTo(writer);
		}
		public BaseManaGroup ReadFrom(BinaryReader reader)
		{
			return ManaGroup.FromBinary(reader);
		}
		public string ToString(BaseManaGroup value)
		{
			return value.ToString();
		}
		public bool IsScalar
		{
			get
			{
				return true;
			}
		}
	}
}
