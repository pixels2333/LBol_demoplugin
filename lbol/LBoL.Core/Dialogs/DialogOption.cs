using System;
using Yarn;

namespace LBoL.Core.Dialogs
{
	// Token: 0x02000122 RID: 290
	public class DialogOption
	{
		// Token: 0x17000343 RID: 835
		// (get) Token: 0x06000A52 RID: 2642 RVA: 0x0001D09B File Offset: 0x0001B29B
		public int Id { get; }

		// Token: 0x17000344 RID: 836
		// (get) Token: 0x06000A53 RID: 2643 RVA: 0x0001D0A3 File Offset: 0x0001B2A3
		public bool Available { get; }

		// Token: 0x17000345 RID: 837
		// (get) Token: 0x06000A54 RID: 2644 RVA: 0x0001D0AB File Offset: 0x0001B2AB
		// (set) Token: 0x06000A55 RID: 2645 RVA: 0x0001D0B3 File Offset: 0x0001B2B3
		public DialogOptionData Data { get; set; }

		// Token: 0x06000A56 RID: 2646 RVA: 0x0001D0BC File Offset: 0x0001B2BC
		internal DialogOption(Line line, int id, bool available)
		{
			this._lineId = line.ID;
			this._args = line.Substitutions;
			this.Id = id;
			this.Available = available;
		}

		// Token: 0x06000A57 RID: 2647 RVA: 0x0001D0EA File Offset: 0x0001B2EA
		public string GetLocalizedText(DialogRunner runner)
		{
			return runner.LocalizeLine(this._lineId, this._args);
		}

		// Token: 0x04000514 RID: 1300
		private readonly string _lineId;

		// Token: 0x04000515 RID: 1301
		private readonly string[] _args;
	}
}
