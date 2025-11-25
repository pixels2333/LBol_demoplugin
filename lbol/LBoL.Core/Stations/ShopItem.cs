using System;
namespace LBoL.Core.Stations
{
	public sealed class ShopItem<T> where T : GameEntity
	{
		public T Content { get; }
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
		public bool IsSoldOut { get; internal set; }
		public bool IsDiscounted { get; internal set; }
		public ShopItem(GameRunController gameRun, T content, int price, bool isSoldOut = false, bool isDiscounted = false)
		{
			this._gameRun = new WeakReference<GameRunController>(gameRun);
			content.GameRun = gameRun;
			this.Content = content;
			this._basePrice = price;
			this.IsSoldOut = isSoldOut;
			this.IsDiscounted = isDiscounted;
		}
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
		private readonly WeakReference<GameRunController> _gameRun;
		private readonly int _basePrice;
	}
}
