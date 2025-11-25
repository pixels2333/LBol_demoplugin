using System;
namespace LBoL.Core
{
	[AttributeUsage(64)]
	public class RuntimeCommandAttribute : Attribute
	{
		public string Name { get; }
		public string Description { get; }
		public RuntimeCommandAttribute(string name, string description = "")
		{
			this.Name = name;
			this.Description = description;
		}
	}
}
