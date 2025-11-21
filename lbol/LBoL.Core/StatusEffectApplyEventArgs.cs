using System;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x0200002B RID: 43
	public class StatusEffectApplyEventArgs : GameEventArgs
	{
		// Token: 0x17000065 RID: 101
		// (get) Token: 0x06000150 RID: 336 RVA: 0x00004332 File Offset: 0x00002532
		// (set) Token: 0x06000151 RID: 337 RVA: 0x0000433A File Offset: 0x0000253A
		public Unit Unit { get; internal set; }

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x06000152 RID: 338 RVA: 0x00004343 File Offset: 0x00002543
		// (set) Token: 0x06000153 RID: 339 RVA: 0x0000434B File Offset: 0x0000254B
		public StatusEffect Effect { get; internal set; }

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x06000154 RID: 340 RVA: 0x00004354 File Offset: 0x00002554
		// (set) Token: 0x06000155 RID: 341 RVA: 0x0000435C File Offset: 0x0000255C
		public int? Level { get; set; }

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x06000156 RID: 342 RVA: 0x00004365 File Offset: 0x00002565
		// (set) Token: 0x06000157 RID: 343 RVA: 0x0000436D File Offset: 0x0000256D
		public int? Duration { get; set; }

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x06000158 RID: 344 RVA: 0x00004376 File Offset: 0x00002576
		// (set) Token: 0x06000159 RID: 345 RVA: 0x0000437E File Offset: 0x0000257E
		public int? Count { get; set; }

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x0600015A RID: 346 RVA: 0x00004387 File Offset: 0x00002587
		// (set) Token: 0x0600015B RID: 347 RVA: 0x0000438F File Offset: 0x0000258F
		public StatusEffectAddResult? AddResult { get; internal set; }

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x0600015C RID: 348 RVA: 0x00004398 File Offset: 0x00002598
		// (set) Token: 0x0600015D RID: 349 RVA: 0x000043A0 File Offset: 0x000025A0
		public float WaitTime { get; set; }

		// Token: 0x0600015E RID: 350 RVA: 0x000043AC File Offset: 0x000025AC
		protected override string GetBaseDebugString()
		{
			StatusEffectAddResult? addResult = this.AddResult;
			if (addResult != null)
			{
				StatusEffectAddResult valueOrDefault = addResult.GetValueOrDefault();
				return string.Format("{0} -> {1} (result: {2})", GameEventArgs.DebugString(this.Effect), GameEventArgs.DebugString(this.Unit), valueOrDefault);
			}
			return GameEventArgs.DebugString(this.Effect) + " -> " + GameEventArgs.DebugString(this.Unit);
		}
	}
}
