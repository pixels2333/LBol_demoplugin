using System;
using System.Diagnostics.CodeAnalysis;

namespace LBoL.Core
{
	// Token: 0x0200000D RID: 13
	public class ExhibitInfoAttribute : Attribute
	{
		// Token: 0x17000025 RID: 37
		// (get) Token: 0x06000081 RID: 129 RVA: 0x0000303B File Offset: 0x0000123B
		// (set) Token: 0x06000082 RID: 130 RVA: 0x00003043 File Offset: 0x00001243
		public int ExpireStageLevel { get; set; } = int.MaxValue;

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x06000083 RID: 131 RVA: 0x0000304C File Offset: 0x0000124C
		// (set) Token: 0x06000084 RID: 132 RVA: 0x00003054 File Offset: 0x00001254
		public int ExpireStationLevel { get; set; } = int.MaxValue;

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000085 RID: 133 RVA: 0x0000305D File Offset: 0x0000125D
		// (set) Token: 0x06000086 RID: 134 RVA: 0x00003065 File Offset: 0x00001265
		public Type WeighterType { get; set; }

		// Token: 0x06000087 RID: 135 RVA: 0x00003070 File Offset: 0x00001270
		internal IExhibitWeighter CreateWeighter()
		{
			if (this.ExpireStageLevel == 2147483647 && this.ExpireStationLevel == 2147483647 && this.WeighterType == null)
			{
				return null;
			}
			return new ExhibitInfoAttribute.ExpireWrappedExhibitWeighter((this.WeighterType != null) ? ((IExhibitWeighter)Activator.CreateInstance(this.WeighterType)) : null, this.ExpireStageLevel, this.ExpireStationLevel);
		}

		// Token: 0x020001CB RID: 459
		internal sealed class ExpireWrappedExhibitWeighter : IExhibitWeighter
		{
			// Token: 0x06000FFF RID: 4095 RVA: 0x0002ADB8 File Offset: 0x00028FB8
			public ExpireWrappedExhibitWeighter(IExhibitWeighter inner, int stageLevel, int stationLevel)
			{
				this._inner = inner;
				this._stageLevel = stageLevel;
				this._stationLevel = stationLevel;
			}

			// Token: 0x06001000 RID: 4096 RVA: 0x0002ADE8 File Offset: 0x00028FE8
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

			// Token: 0x0400072C RID: 1836
			[MaybeNull]
			private readonly IExhibitWeighter _inner;

			// Token: 0x0400072D RID: 1837
			private readonly int _stageLevel;

			// Token: 0x0400072E RID: 1838
			private readonly int _stationLevel;
		}
	}
}
