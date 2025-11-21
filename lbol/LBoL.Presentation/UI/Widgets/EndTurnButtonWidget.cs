using System;
using LBoL.Presentation.UI.Panels;
using UnityEngine.EventSystems;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000053 RID: 83
	public class EndTurnButtonWidget : CommonButtonWidget
	{
		// Token: 0x060004CC RID: 1228 RVA: 0x00013F0B File Offset: 0x0001210B
		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);
			UiManager.GetPanel<PlayBoard>().CardUi.StartCardsEndNotify();
		}
	}
}
