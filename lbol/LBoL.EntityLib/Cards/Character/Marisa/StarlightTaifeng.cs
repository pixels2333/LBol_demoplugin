using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x0200044A RID: 1098
	[UsedImplicitly]
	public sealed class StarlightTaifeng : Card
	{
		// Token: 0x06000EF0 RID: 3824 RVA: 0x0001B16E File Offset: 0x0001936E
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}
	}
}
