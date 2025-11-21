using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x02000401 RID: 1025
	[UsedImplicitly]
	public sealed class TuimofuLuanwu : Card
	{
		// Token: 0x06000E32 RID: 3634 RVA: 0x0001A3AC File Offset: 0x000185AC
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}
	}
}
