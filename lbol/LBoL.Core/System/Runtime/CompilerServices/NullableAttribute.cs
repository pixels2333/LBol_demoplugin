using System;
using Microsoft.CodeAnalysis;
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Embedded]
	[AttributeUsage(27524, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableAttribute : Attribute
	{
		public NullableAttribute(byte A_1)
		{
			this.NullableFlags = new byte[] { A_1 };
		}
		public NullableAttribute(byte[] A_1)
		{
			this.NullableFlags = A_1;
		}
		public readonly byte[] NullableFlags;
	}
}
