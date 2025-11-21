using System;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000159 RID: 345
	internal class ActionViewerTypeAttribute : Attribute
	{
		// Token: 0x170004D1 RID: 1233
		// (get) Token: 0x06000DAE RID: 3502 RVA: 0x00025A3B File Offset: 0x00023C3B
		public Type Type { get; }

		// Token: 0x06000DAF RID: 3503 RVA: 0x00025A43 File Offset: 0x00023C43
		public ActionViewerTypeAttribute(Type type)
		{
			this.Type = type;
		}
	}
}
