using System;
using LBoL.Presentation.UI.Panels;
using UnityEngine.EventSystems;
namespace LBoL.Presentation.UI.Widgets
{
	public class EndTurnButtonWidget : CommonButtonWidget
	{
		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);
			UiManager.GetPanel<PlayBoard>().CardUi.StartCardsEndNotify();
		}
	}
}
