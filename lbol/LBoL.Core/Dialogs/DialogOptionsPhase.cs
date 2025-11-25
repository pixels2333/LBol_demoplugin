using System;
using System.Collections.Generic;
using System.Linq;
namespace LBoL.Core.Dialogs
{
	public sealed class DialogOptionsPhase : DialogPhase
	{
		public DialogOption[] Options { get; }
		internal DialogOptionsPhase(IEnumerable<DialogOption> options)
		{
			this.Options = Enumerable.ToArray<DialogOption>(options);
		}
		public override string ToString()
		{
			return string.Format("Options: [{0}]", this.Options.Length);
		}
	}
}
