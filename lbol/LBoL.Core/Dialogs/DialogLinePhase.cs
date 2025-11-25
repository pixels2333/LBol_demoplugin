using System;
using Yarn;
namespace LBoL.Core.Dialogs
{
	public class DialogLinePhase : DialogPhase
	{
		internal DialogLinePhase(Line line)
		{
			this._lineId = line.ID;
			this._args = line.Substitutions;
		}
		public string GetLocalizedText(DialogRunner runner)
		{
			return runner.LocalizeLine(this._lineId, this._args);
		}
		private readonly string _lineId;
		private readonly string[] _args;
	}
}
