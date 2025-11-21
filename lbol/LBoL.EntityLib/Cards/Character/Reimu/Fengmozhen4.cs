using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003D2 RID: 978
	[UsedImplicitly]
	public sealed class Fengmozhen4 : Card
	{
		// Token: 0x06000DC2 RID: 3522 RVA: 0x00019B2C File Offset: 0x00017D2C
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}
	}
}
