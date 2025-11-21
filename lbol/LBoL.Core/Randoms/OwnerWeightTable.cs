using System;

namespace LBoL.Core.Randoms
{
	// Token: 0x020000EF RID: 239
	public sealed class OwnerWeightTable
	{
		// Token: 0x06000931 RID: 2353 RVA: 0x0001AC05 File Offset: 0x00018E05
		public OwnerWeightTable(float player, float exhibitOwner, float other, float neutral)
		{
			this.Player = player;
			this.ExhibitOwner = exhibitOwner;
			this.Other = other;
			this.Neutral = neutral;
		}

		// Token: 0x170002EE RID: 750
		// (get) Token: 0x06000932 RID: 2354 RVA: 0x0001AC2A File Offset: 0x00018E2A
		public float Player { get; }

		// Token: 0x170002EF RID: 751
		// (get) Token: 0x06000933 RID: 2355 RVA: 0x0001AC32 File Offset: 0x00018E32
		public float ExhibitOwner { get; }

		// Token: 0x170002F0 RID: 752
		// (get) Token: 0x06000934 RID: 2356 RVA: 0x0001AC3A File Offset: 0x00018E3A
		public float Other { get; }

		// Token: 0x170002F1 RID: 753
		// (get) Token: 0x06000935 RID: 2357 RVA: 0x0001AC42 File Offset: 0x00018E42
		public float Neutral { get; }

		// Token: 0x040004DB RID: 1243
		public static readonly OwnerWeightTable AllOnes = new OwnerWeightTable(1f, 1f, 1f, 1f);

		// Token: 0x040004DC RID: 1244
		public static readonly OwnerWeightTable Valid = new OwnerWeightTable(1f, 1f, 0f, 1f);

		// Token: 0x040004DD RID: 1245
		public static readonly OwnerWeightTable OnlyPlayer = new OwnerWeightTable(1f, 0f, 0f, 0f);

		// Token: 0x040004DE RID: 1246
		public static readonly OwnerWeightTable OnlyFriend = new OwnerWeightTable(0f, 1f, 0f, 0f);

		// Token: 0x040004DF RID: 1247
		public static readonly OwnerWeightTable OnlyOther = new OwnerWeightTable(0f, 0f, 1f, 0f);

		// Token: 0x040004E0 RID: 1248
		public static readonly OwnerWeightTable OnlyNeutral = new OwnerWeightTable(0f, 0f, 0f, 1f);

		// Token: 0x040004E1 RID: 1249
		public static readonly OwnerWeightTable WithoutPlayer = new OwnerWeightTable(0f, 1f, 0f, 1f);

		// Token: 0x040004E2 RID: 1250
		public static readonly OwnerWeightTable PlayerAndFriend = new OwnerWeightTable(0.2f, 0.8f, 0f, 0f);

		// Token: 0x040004E3 RID: 1251
		public static readonly OwnerWeightTable Hierarchy = new OwnerWeightTable(1f, 0.8f, 0f, 0.7f);
	}
}
