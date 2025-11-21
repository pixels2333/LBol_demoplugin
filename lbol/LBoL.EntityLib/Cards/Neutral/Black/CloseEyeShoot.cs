using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x0200032D RID: 813
	[UsedImplicitly]
	public sealed class CloseEyeShoot : Card
	{
		// Token: 0x06000BEB RID: 3051 RVA: 0x00017902 File Offset: 0x00015B02
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}
	}
}
