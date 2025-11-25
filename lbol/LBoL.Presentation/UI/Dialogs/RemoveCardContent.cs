using System;
using LBoL.Core.Cards;
namespace LBoL.Presentation.UI.Dialogs
{
	public sealed class RemoveCardContent
	{
		public Card Card { get; set; }
		public Action OnConfirm { get; set; }
	}
}
