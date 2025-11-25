using System;
namespace LBoL.Core.Dialogs
{
	public class DialogCommandPhase : DialogPhase
	{
		public string Text { get; }
		internal DialogCommandPhase(string text)
		{
			this.Text = text;
		}
		public override string ToString()
		{
			return "Command: " + this.Text;
		}
	}
}
