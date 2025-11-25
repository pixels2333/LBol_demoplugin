using System;
using JetBrains.Annotations;
namespace LBoL.Core.Battle
{
	public abstract class Interaction
	{
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
		[CanBeNull]
		public string Description { get; set; }
		public bool CanCancel { get; set; }
		public bool IsCanceled { get; private set; }
		public void Cancel()
		{
			if (!this.CanCancel)
			{
				throw new InvalidOperationException(string.Format("Cannot cancel {0}", this));
			}
			this.IsCanceled = true;
		}
		private readonly WeakReference<GameEntity> _source = new WeakReference<GameEntity>(null);
	}
}
