using System;
using JetBrains.Annotations;

namespace LBoL.Core.Battle
{
	// Token: 0x02000145 RID: 325
	public abstract class Interaction
	{
		// Token: 0x17000492 RID: 1170
		// (get) Token: 0x06000D18 RID: 3352 RVA: 0x00025190 File Offset: 0x00023390
		// (set) Token: 0x06000D19 RID: 3353 RVA: 0x000251AF File Offset: 0x000233AF
		public GameEntity Source
		{
			get
			{
				GameEntity gameEntity;
				if (!this._source.TryGetTarget(ref gameEntity))
				{
					return null;
				}
				return gameEntity;
			}
			set
			{
				this._source.SetTarget(value);
			}
		}

		// Token: 0x17000493 RID: 1171
		// (get) Token: 0x06000D1A RID: 3354 RVA: 0x000251BD File Offset: 0x000233BD
		// (set) Token: 0x06000D1B RID: 3355 RVA: 0x000251C5 File Offset: 0x000233C5
		[CanBeNull]
		public string Description { get; set; }

		// Token: 0x17000494 RID: 1172
		// (get) Token: 0x06000D1C RID: 3356 RVA: 0x000251CE File Offset: 0x000233CE
		// (set) Token: 0x06000D1D RID: 3357 RVA: 0x000251D6 File Offset: 0x000233D6
		public bool CanCancel { get; set; }

		// Token: 0x17000495 RID: 1173
		// (get) Token: 0x06000D1E RID: 3358 RVA: 0x000251DF File Offset: 0x000233DF
		// (set) Token: 0x06000D1F RID: 3359 RVA: 0x000251E7 File Offset: 0x000233E7
		public bool IsCanceled { get; private set; }

		// Token: 0x06000D20 RID: 3360 RVA: 0x000251F0 File Offset: 0x000233F0
		public void Cancel()
		{
			if (!this.CanCancel)
			{
				throw new InvalidOperationException(string.Format("Cannot cancel {0}", this));
			}
			this.IsCanceled = true;
		}

		// Token: 0x04000620 RID: 1568
		private readonly WeakReference<GameEntity> _source = new WeakReference<GameEntity>(null);
	}
}
