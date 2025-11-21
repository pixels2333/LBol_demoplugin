using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003D1 RID: 977
	[UsedImplicitly]
	public sealed class Fengmozhen : Card
	{
		// Token: 0x1700018A RID: 394
		// (get) Token: 0x06000DBE RID: 3518 RVA: 0x00019AE0 File Offset: 0x00017CE0
		// (set) Token: 0x06000DBF RID: 3519 RVA: 0x00019AE8 File Offset: 0x00017CE8
		public bool IsPhantom { get; set; }

		// Token: 0x06000DC0 RID: 3520 RVA: 0x00019AF1 File Offset: 0x00017CF1
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(this.IsPhantom ? (this.IsUpgraded ? "幻影针" : "幻影针B") : base.GunName, 1, true);
		}
	}
}
