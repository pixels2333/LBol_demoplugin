using System;
namespace LBoL.Presentation.UI.Dialogs
{
	public sealed class MessageContent
	{
		public string Text { get; set; }
		public string TextKey { get; set; }
		public object[] TextArguments { get; set; }
		public string SubText { get; set; }
		public string SubTextKey { get; set; }
		public object[] SubTextArguments { get; set; }
		public DialogButtons Buttons { get; set; }
		public MessageIcon Icon { get; set; }
		public Action OnConfirm { get; set; }
		public Action OnCancel { get; set; }
		private string _text;
		private string _textKey;
		private string _subText;
		private string _subTextKey;
	}
}
