using System;
using Yarn;

namespace LBoL.Core.Dialogs
{
	// Token: 0x02000121 RID: 289
	public class DialogLinePhase : DialogPhase
	{
		// Token: 0x06000A50 RID: 2640 RVA: 0x0001D067 File Offset: 0x0001B267
		internal DialogLinePhase(Line line)
		{
			this._lineId = line.ID;
			this._args = line.Substitutions;
		}

		// Token: 0x06000A51 RID: 2641 RVA: 0x0001D087 File Offset: 0x0001B287
		public string GetLocalizedText(DialogRunner runner)
		{
			return runner.LocalizeLine(this._lineId, this._args);
		}

		// Token: 0x04000512 RID: 1298
		private readonly string _lineId;

		// Token: 0x04000513 RID: 1299
		private readonly string[] _args;
	}
}
