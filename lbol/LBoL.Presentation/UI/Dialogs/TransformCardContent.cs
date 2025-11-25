using System;
using LBoL.Core.Cards;
namespace LBoL.Presentation.UI.Dialogs
{
	public sealed class TransformCardContent
	{
		public Card Card { get; set; }
		public Card TransformCard { get; set; }
		public Action OnConfirm { get; set; }
	}
}
