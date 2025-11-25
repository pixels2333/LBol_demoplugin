using System;
using System.Diagnostics.CodeAnalysis;
namespace LBoL.Core
{
	public class ExhibitInfoAttribute : Attribute
	{
		public int ExpireStageLevel { get; set; } = int.MaxValue;
		public int ExpireStationLevel { get; set; } = int.MaxValue;
		public Type WeighterType { get; set; }
		internal IExhibitWeighter CreateWeighter()
		{
			if (this.ExpireStageLevel == 2147483647 && this.ExpireStationLevel == 2147483647 && this.WeighterType == null)
			{
				return null;
			}
			return new ExhibitInfoAttribute.ExpireWrappedExhibitWeighter((this.WeighterType != null) ? ((IExhibitWeighter)Activator.CreateInstance(this.WeighterType)) : null, this.ExpireStageLevel, this.ExpireStationLevel);
		}
		internal sealed class ExpireWrappedExhibitWeighter : IExhibitWeighter
		{
			public ExpireWrappedExhibitWeighter(IExhibitWeighter inner, int stageLevel, int stationLevel)
			{
				this._inner = inner;
				this._stageLevel = stageLevel;
				this._stationLevel = stationLevel;
			}
			public float WeightFor(Type type, GameRunController gameRun)
			{
				if (gameRun.CurrentStage.Level > this._stageLevel)
				{
					return 0f;
				}
				if (gameRun.CurrentStage.Level == this._stageLevel && gameRun.CurrentStation.Level > this._stationLevel)
				{
					return 0f;
				}
				IExhibitWeighter inner = this._inner;
				if (inner == null)
				{
					return 1f;
				}
				return inner.WeightFor(type, gameRun);
			}
			[MaybeNull]
			private readonly IExhibitWeighter _inner;
			private readonly int _stageLevel;
			private readonly int _stationLevel;
		}
	}
}
