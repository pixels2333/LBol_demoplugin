using System;

namespace LBoL.Core.Stations
{
	// Token: 0x020000C4 RID: 196
	public sealed class ShopItem<T> where T : GameEntity
	{
		// Token: 0x170002B3 RID: 691
		// (get) Token: 0x06000870 RID: 2160 RVA: 0x00018B71 File Offset: 0x00016D71
		public T Content { get; }

		// Token: 0x170002B4 RID: 692
		// (get) Token: 0x06000871 RID: 2161 RVA: 0x00018B7C File Offset: 0x00016D7C
		public int Price
		{
			get
			{
				GameRunController gameRunController;
				if (!this._gameRun.TryGetTarget(ref gameRunController))
				{
					return this._basePrice;
				}
				return (int)(gameRunController.FinalShopPriceMultiplier * (float)this._basePrice);
			}
		}

		// Token: 0x170002B5 RID: 693
		// (get) Token: 0x06000872 RID: 2162 RVA: 0x00018BAE File Offset: 0x00016DAE
		// (set) Token: 0x06000873 RID: 2163 RVA: 0x00018BB6 File Offset: 0x00016DB6
		public bool IsSoldOut { get; internal set; }

		// Token: 0x170002B6 RID: 694
		// (get) Token: 0x06000874 RID: 2164 RVA: 0x00018BBF File Offset: 0x00016DBF
		// (set) Token: 0x06000875 RID: 2165 RVA: 0x00018BC7 File Offset: 0x00016DC7
		public bool IsDiscounted { get; internal set; }

		// Token: 0x06000876 RID: 2166 RVA: 0x00018BD0 File Offset: 0x00016DD0
		public ShopItem(GameRunController gameRun, T content, int price, bool isSoldOut = false, bool isDiscounted = false)
		{
			this._gameRun = new WeakReference<GameRunController>(gameRun);
			content.GameRun = gameRun;
			this.Content = content;
			this._basePrice = price;
			this.IsSoldOut = isSoldOut;
			this.IsDiscounted = isDiscounted;
		}

		// Token: 0x06000877 RID: 2167 RVA: 0x00018C10 File Offset: 0x00016E10
		public override string ToString()
		{
			return string.Format("{{{0}<{1}> {2} ({3})}}", new object[]
			{
				"ShopItem",
				typeof(T).Name,
				this.Content.Name,
				this.Price
			});
		}

		// Token: 0x04000399 RID: 921
		private readonly WeakReference<GameRunController> _gameRun;

		// Token: 0x0400039A RID: 922
		private readonly int _basePrice;
	}
}
