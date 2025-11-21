using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003DA RID: 986
	[UsedImplicitly]
	public sealed class JiejieAttack : Card
	{
		// Token: 0x1700018B RID: 395
		// (get) Token: 0x06000DD3 RID: 3539 RVA: 0x00019C06 File Offset: 0x00017E06
		private string Gun1
		{
			get
			{
				if (!this.IsUpgraded)
				{
					return "结界猛击";
				}
				return "结界猛击B";
			}
		}

		// Token: 0x1700018C RID: 396
		// (get) Token: 0x06000DD4 RID: 3540 RVA: 0x00019C1B File Offset: 0x00017E1B
		private string Gun2
		{
			get
			{
				if (!this.IsUpgraded)
				{
					return "结界猛击2";
				}
				return "结界猛击2B";
			}
		}

		// Token: 0x1700018D RID: 397
		// (get) Token: 0x06000DD5 RID: 3541 RVA: 0x00019C30 File Offset: 0x00017E30
		[UsedImplicitly]
		public int ShieldDamage
		{
			get
			{
				if (base.Battle != null)
				{
					return base.Battle.Player.Shield;
				}
				return 0;
			}
		}

		// Token: 0x1700018E RID: 398
		// (get) Token: 0x06000DD6 RID: 3542 RVA: 0x00019C4C File Offset: 0x00017E4C
		protected override int AdditionalDamage
		{
			get
			{
				return this.ShieldDamage;
			}
		}

		// Token: 0x06000DD7 RID: 3543 RVA: 0x00019C54 File Offset: 0x00017E54
		protected override void SetGuns()
		{
			base.CardGuns = ((this.ShieldDamage > 0) ? new Guns(this.Gun2) : new Guns(this.Gun1));
		}
	}
}
