using System;

namespace LBoL.Core.Dialogs
{
	// Token: 0x0200011E RID: 286
	public class DialogCommandPhase : DialogPhase
	{
		// Token: 0x17000341 RID: 833
		// (get) Token: 0x06000A2D RID: 2605 RVA: 0x0001CCAE File Offset: 0x0001AEAE
		public string Text { get; }

		// Token: 0x06000A2E RID: 2606 RVA: 0x0001CCB6 File Offset: 0x0001AEB6
		internal DialogCommandPhase(string text)
		{
			this.Text = text;
		}

		// Token: 0x06000A2F RID: 2607 RVA: 0x0001CCC5 File Offset: 0x0001AEC5
		public override string ToString()
		{
			return "Command: " + this.Text;
		}
	}
}
