using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002A7 RID: 679
	[UsedImplicitly]
	public sealed class SanaeMiracle : Card
	{
		// Token: 0x17000134 RID: 308
		// (get) Token: 0x06000A88 RID: 2696 RVA: 0x00015D08 File Offset: 0x00013F08
		public override bool Triggered
		{
			get
			{
				return this.IsForceCost;
			}
		}

		// Token: 0x17000135 RID: 309
		// (get) Token: 0x06000A89 RID: 2697 RVA: 0x00015D10 File Offset: 0x00013F10
		public override bool IsForceCost
		{
			get
			{
				return base.Battle != null && base.Battle.DrawZone.Count == 0;
			}
		}

		// Token: 0x06000A8A RID: 2698 RVA: 0x00015D2F File Offset: 0x00013F2F
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}
	}
}
