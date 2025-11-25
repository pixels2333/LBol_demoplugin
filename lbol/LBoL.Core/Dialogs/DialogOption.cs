using System;
using Yarn;
namespace LBoL.Core.Dialogs
{
	public class DialogOption
	{
		public int Id { get; }
		public bool Available { get; }
		public DialogOptionData Data { get; set; }
		internal DialogOption(Line line, int id, bool available)
		{
			this._lineId = line.ID;
			this._args = line.Substitutions;
			this.Id = id;
			this.Available = available;
		}
		public string GetLocalizedText(DialogRunner runner)
		{
			return runner.LocalizeLine(this._lineId, this._args);
		}
		private readonly string _lineId;
		private readonly string[] _args;
	}
}
