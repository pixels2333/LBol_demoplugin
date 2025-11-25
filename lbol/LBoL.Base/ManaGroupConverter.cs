using System;
using System.IO;
using Untitled.ConfigDataBuilder.Base;
namespace LBoL.Base
{
	public sealed class ManaGroupConverter : IConfigValueConverter<ManaGroup>
	{
		public ManaGroup Parse(string value)
		{
			return ManaGroup.Parse(value);
		}
		public void WriteTo(BinaryWriter writer, ManaGroup value)
		{
			value.WriteTo(writer);
		}
		public ManaGroup ReadFrom(BinaryReader reader)
		{
			return ManaGroup.FromBinary(reader);
		}
		public string ToString(ManaGroup value)
		{
			return value.ToString();
		}
	}
}
