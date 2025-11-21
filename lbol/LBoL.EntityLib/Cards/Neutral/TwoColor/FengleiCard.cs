using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Character.Reimu;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x0200028C RID: 652
	[UsedImplicitly]
	public sealed class FengleiCard : YinyangCardBase
	{
		// Token: 0x06000A3C RID: 2620 RVA: 0x00015744 File Offset: 0x00013944
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}
	}
}
