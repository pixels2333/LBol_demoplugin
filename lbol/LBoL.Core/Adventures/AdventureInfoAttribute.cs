using System;
namespace LBoL.Core.Adventures
{
	public class AdventureInfoAttribute : Attribute
	{
		public Type WeighterType { get; set; }
	}
}
