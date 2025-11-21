using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Alice
{
	// Token: 0x020004E4 RID: 1252
	[UsedImplicitly]
	public sealed class AliceKickerAttack : Card
	{
		// Token: 0x06001098 RID: 4248 RVA: 0x0001D1B2 File Offset: 0x0001B3B2
		protected override void SetGuns()
		{
			base.CardGuns = (base.KickerPlaying ? new Guns(base.GunName, 2, true) : new Guns(base.GunName));
		}
	}
}
