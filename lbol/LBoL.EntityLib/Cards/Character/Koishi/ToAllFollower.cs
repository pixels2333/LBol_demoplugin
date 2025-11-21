using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000496 RID: 1174
	[UsedImplicitly]
	public sealed class ToAllFollower : Card
	{
		// Token: 0x170001B7 RID: 439
		// (get) Token: 0x06000FB1 RID: 4017 RVA: 0x0001BF75 File Offset: 0x0001A175
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && base.Battle.Player.HasStatusEffect<MoodEpiphany>();
			}
		}

		// Token: 0x170001B8 RID: 440
		// (get) Token: 0x06000FB2 RID: 4018 RVA: 0x0001BF91 File Offset: 0x0001A191
		private Guns Guns1
		{
			get
			{
				return new Guns(base.Config.GunName);
			}
		}

		// Token: 0x170001B9 RID: 441
		// (get) Token: 0x06000FB3 RID: 4019 RVA: 0x0001BFA3 File Offset: 0x0001A1A3
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

		// Token: 0x06000FB4 RID: 4020 RVA: 0x0001BFCC File Offset: 0x0001A1CC
		protected override void SetGuns()
		{
			base.CardGuns = (base.TriggeredAnyhow ? this.Guns2 : this.Guns1);
		}
	}
}
