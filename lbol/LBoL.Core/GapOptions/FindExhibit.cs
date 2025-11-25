using System;
using JetBrains.Annotations;
using LBoL.Base;
namespace LBoL.Core.GapOptions
{
	[UsedImplicitly]
	public sealed class FindExhibit : GapOption
	{
		public override GapOptionType Type
		{
			get
			{
				return GapOptionType.FindExhibit;
			}
		}
	}
}
