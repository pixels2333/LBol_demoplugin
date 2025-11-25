using System;
namespace LBoL.Base
{
	public sealed class KeywordAttribute : Attribute
	{
		public const bool DefaultAutoAppend = true;
		public bool AutoAppend = true;
		public bool Hidden;
		public const bool DefaultIsVerbose = false;
		public bool IsVerbose;
	}
}
