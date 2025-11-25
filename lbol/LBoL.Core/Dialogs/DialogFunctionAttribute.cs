using System;
namespace LBoL.Core.Dialogs
{
	public sealed class DialogFunctionAttribute : Attribute
	{
		public string Name { get; }
		public DialogFunctionAttribute(string name)
		{
			this.Name = name;
		}
	}
}
