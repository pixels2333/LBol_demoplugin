using System;
using LBoL.Base;
using LBoL.Core.Cards;

namespace LBoL.Core
{
	// Token: 0x02000021 RID: 33
	public class CardUsingEventArgs : GameEventArgs
	{
		// Token: 0x17000049 RID: 73
		// (get) Token: 0x06000105 RID: 261 RVA: 0x00003F02 File Offset: 0x00002102
		// (set) Token: 0x06000106 RID: 262 RVA: 0x00003F0A File Offset: 0x0000210A
		public Card Card { get; internal set; }

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x06000107 RID: 263 RVA: 0x00003F13 File Offset: 0x00002113
		// (set) Token: 0x06000108 RID: 264 RVA: 0x00003F1B File Offset: 0x0000211B
		public UnitSelector Selector { get; internal set; }

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x06000109 RID: 265 RVA: 0x00003F24 File Offset: 0x00002124
		// (set) Token: 0x0600010A RID: 266 RVA: 0x00003F2C File Offset: 0x0000212C
		public ManaGroup ConsumingMana { get; internal set; }

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x0600010B RID: 267 RVA: 0x00003F35 File Offset: 0x00002135
		// (set) Token: 0x0600010C RID: 268 RVA: 0x00003F3D File Offset: 0x0000213D
		public bool PlayTwice { get; set; }

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x0600010D RID: 269 RVA: 0x00003F46 File Offset: 0x00002146
		// (set) Token: 0x0600010E RID: 270 RVA: 0x00003F4E File Offset: 0x0000214E
		public bool Kicker { get; set; }

		// Token: 0x0600010F RID: 271 RVA: 0x00003F58 File Offset: 0x00002158
		public CardUsingEventArgs Clone()
		{
			return new CardUsingEventArgs
			{
				Card = this.Card,
				Selector = this.Selector,
				ConsumingMana = this.ConsumingMana,
				PlayTwice = this.PlayTwice,
				Kicker = this.Kicker
			};
		}

		// Token: 0x06000110 RID: 272 RVA: 0x00003FA6 File Offset: 0x000021A6
		protected override string GetBaseDebugString()
		{
			return GameEventArgs.DebugString(this.Card) + " -> {" + GameEventArgs.DebugString(this.Selector) + "}";
		}
	}
}
