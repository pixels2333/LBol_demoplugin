using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000487 RID: 1159
	[UsedImplicitly]
	public sealed class MultiFollower : Card
	{
		// Token: 0x170001AE RID: 430
		// (get) Token: 0x06000F80 RID: 3968 RVA: 0x0001BB39 File Offset: 0x00019D39
		public override bool ShuffleToBottom
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000F81 RID: 3969 RVA: 0x0001BB3C File Offset: 0x00019D3C
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, false);
		}
	}
}
