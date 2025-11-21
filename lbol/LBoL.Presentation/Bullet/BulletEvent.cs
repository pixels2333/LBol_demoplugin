using System;

namespace LBoL.Presentation.Bullet
{
	// Token: 0x0200010F RID: 271
	public class BulletEvent
	{
		// Token: 0x06000EDE RID: 3806 RVA: 0x000469C4 File Offset: 0x00044BC4
		public BulletEvent(int eventStart, int eventDuration, float[][] eventNumber, int[] eventType)
		{
			this.EventStart = eventStart;
			this.EventDuration = eventDuration;
			this.EventNumber = eventNumber;
			this.EventType = eventType;
		}

		// Token: 0x17000289 RID: 649
		// (get) Token: 0x06000EDF RID: 3807 RVA: 0x000469E9 File Offset: 0x00044BE9
		public int EventStart { get; }

		// Token: 0x1700028A RID: 650
		// (get) Token: 0x06000EE0 RID: 3808 RVA: 0x000469F1 File Offset: 0x00044BF1
		public int EventDuration { get; }

		// Token: 0x1700028B RID: 651
		// (get) Token: 0x06000EE1 RID: 3809 RVA: 0x000469F9 File Offset: 0x00044BF9
		public float[][] EventNumber { get; }

		// Token: 0x1700028C RID: 652
		// (get) Token: 0x06000EE2 RID: 3810 RVA: 0x00046A01 File Offset: 0x00044C01
		// (set) Token: 0x06000EE3 RID: 3811 RVA: 0x00046A09 File Offset: 0x00044C09
		public float RuntimeEventNumber { get; set; }

		// Token: 0x1700028D RID: 653
		// (get) Token: 0x06000EE4 RID: 3812 RVA: 0x00046A12 File Offset: 0x00044C12
		public int[] EventType { get; }

		// Token: 0x1700028E RID: 654
		// (get) Token: 0x06000EE5 RID: 3813 RVA: 0x00046A1A File Offset: 0x00044C1A
		// (set) Token: 0x06000EE6 RID: 3814 RVA: 0x00046A22 File Offset: 0x00044C22
		public bool EvSetup { get; set; }

		// Token: 0x1700028F RID: 655
		// (get) Token: 0x06000EE7 RID: 3815 RVA: 0x00046A2B File Offset: 0x00044C2B
		// (set) Token: 0x06000EE8 RID: 3816 RVA: 0x00046A33 File Offset: 0x00044C33
		public int EvTimer { get; set; }

		// Token: 0x17000290 RID: 656
		// (get) Token: 0x06000EE9 RID: 3817 RVA: 0x00046A3C File Offset: 0x00044C3C
		// (set) Token: 0x06000EEA RID: 3818 RVA: 0x00046A44 File Offset: 0x00044C44
		public float Delta { get; set; }
	}
}
