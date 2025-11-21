using System;

namespace LBoL.Presentation.UI
{
	// Token: 0x0200002B RID: 43
	public class TooltipPosition
	{
		// Token: 0x17000084 RID: 132
		// (get) Token: 0x06000331 RID: 817 RVA: 0x0000DEE9 File Offset: 0x0000C0E9
		public TooltipDirection Direction { get; }

		// Token: 0x17000085 RID: 133
		// (get) Token: 0x06000332 RID: 818 RVA: 0x0000DEF1 File Offset: 0x0000C0F1
		public TooltipAlignment Alignment { get; }

		// Token: 0x06000333 RID: 819 RVA: 0x0000DEF9 File Offset: 0x0000C0F9
		public TooltipPosition(TooltipDirection direction, TooltipAlignment alignment)
		{
			this.Direction = direction;
			this.Alignment = alignment;
		}
	}
}
