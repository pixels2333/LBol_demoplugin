using System;
using LBoL.Core.Cards;
namespace LBoL.Presentation.UI.Dialogs
{
	public sealed class UpgradeCardContent
	{
		public Card Card { get; set; }
		public int Price { get; set; }
		public int Money { get; set; }
		public Action OnConfirm { get; set; }
	}
}
