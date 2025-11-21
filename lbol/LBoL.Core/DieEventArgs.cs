using System;
using System.Text;
using LBoL.Core.Battle;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x02000033 RID: 51
	public class DieEventArgs : GameEventArgs
	{
		// Token: 0x1700007E RID: 126
		// (get) Token: 0x06000191 RID: 401 RVA: 0x000046C4 File Offset: 0x000028C4
		// (set) Token: 0x06000192 RID: 402 RVA: 0x000046CC File Offset: 0x000028CC
		public Unit Source { get; internal set; }

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x06000193 RID: 403 RVA: 0x000046D5 File Offset: 0x000028D5
		// (set) Token: 0x06000194 RID: 404 RVA: 0x000046DD File Offset: 0x000028DD
		public Unit Unit { get; internal set; }

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x06000195 RID: 405 RVA: 0x000046E6 File Offset: 0x000028E6
		// (set) Token: 0x06000196 RID: 406 RVA: 0x000046EE File Offset: 0x000028EE
		public DieCause DieCause { get; internal set; }

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x06000197 RID: 407 RVA: 0x000046F7 File Offset: 0x000028F7
		// (set) Token: 0x06000198 RID: 408 RVA: 0x000046FF File Offset: 0x000028FF
		public GameEntity DieSource { get; internal set; }

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x06000199 RID: 409 RVA: 0x00004708 File Offset: 0x00002908
		// (set) Token: 0x0600019A RID: 410 RVA: 0x00004710 File Offset: 0x00002910
		public int Power { get; set; }

		// Token: 0x17000083 RID: 131
		// (get) Token: 0x0600019B RID: 411 RVA: 0x00004719 File Offset: 0x00002919
		// (set) Token: 0x0600019C RID: 412 RVA: 0x00004721 File Offset: 0x00002921
		public int BluePoint { get; set; }

		// Token: 0x17000084 RID: 132
		// (get) Token: 0x0600019D RID: 413 RVA: 0x0000472A File Offset: 0x0000292A
		// (set) Token: 0x0600019E RID: 414 RVA: 0x00004732 File Offset: 0x00002932
		public int Money { get; set; }

		// Token: 0x0600019F RID: 415 RVA: 0x0000473C File Offset: 0x0000293C
		protected override string GetBaseDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder().Append(GameEventArgs.DebugString(this.Unit)).Append(" (by ").Append(GameEventArgs.DebugString(this.Source))
				.Append(", ")
				.Append(this.DieCause)
				.Append(", with ")
				.Append(GameEventArgs.DebugString(this.DieSource));
			if (this.Power > 0)
			{
				stringBuilder.Append(", P: ").Append(this.Power);
			}
			if (this.BluePoint > 0)
			{
				stringBuilder.Append(", B: ").Append(this.BluePoint);
			}
			if (this.Money > 0)
			{
				stringBuilder.Append(", M: ").Append(this.Money);
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}
	}
}
