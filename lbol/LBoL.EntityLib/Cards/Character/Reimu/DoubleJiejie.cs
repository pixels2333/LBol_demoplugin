using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003CE RID: 974
	[UsedImplicitly]
	public sealed class DoubleJiejie : Card
	{
		// Token: 0x17000188 RID: 392
		// (get) Token: 0x06000DB6 RID: 3510 RVA: 0x00019A44 File Offset: 0x00017C44
		private Guns Guns1
		{
			get
			{
				return new Guns(base.Config.GunName);
			}
		}

		// Token: 0x17000189 RID: 393
		// (get) Token: 0x06000DB7 RID: 3511 RVA: 0x00019A56 File Offset: 0x00017C56
		private Guns Guns2
		{
			get
			{
				return new Guns(new string[]
				{
					base.Config.GunName,
					base.Config.GunNameBurst
				});
			}
		}

		// Token: 0x06000DB8 RID: 3512 RVA: 0x00019A7F File Offset: 0x00017C7F
		protected override void SetGuns()
		{
			base.CardGuns = ((base.Battle.Player.Shield > 0) ? this.Guns2 : this.Guns1);
		}
	}
}
