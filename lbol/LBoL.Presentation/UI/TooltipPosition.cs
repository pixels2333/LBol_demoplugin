using System;
namespace LBoL.Presentation.UI
{
	public class TooltipPosition
	{
		public TooltipDirection Direction { get; }
		public TooltipAlignment Alignment { get; }
		public TooltipPosition(TooltipDirection direction, TooltipAlignment alignment)
		{
			this.Direction = direction;
			this.Alignment = alignment;
		}
	}
}
