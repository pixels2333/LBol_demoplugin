using System;
using LBoL.Core.Units;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using UnityEngine;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000C2 RID: 194
	[AddComponentMenu(null)]
	public class UnitStatusHud : UiPanel
	{
		// Token: 0x170001C3 RID: 451
		// (get) Token: 0x06000B89 RID: 2953 RVA: 0x0003C18F File Offset: 0x0003A38F
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Bottom;
			}
		}

		// Token: 0x06000B8A RID: 2954 RVA: 0x0003C192 File Offset: 0x0003A392
		public void HideAll()
		{
			this.mainLayer.gameObject.SetActive(false);
			this.chatLayer.gameObject.SetActive(false);
		}

		// Token: 0x06000B8B RID: 2955 RVA: 0x0003C1B6 File Offset: 0x0003A3B6
		public void RevealAll()
		{
			this.mainLayer.gameObject.SetActive(true);
			this.chatLayer.gameObject.SetActive(true);
		}

		// Token: 0x06000B8C RID: 2956 RVA: 0x0003C1DA File Offset: 0x0003A3DA
		public UnitStatusWidget CreateStatusWidget(Unit unit)
		{
			UnitStatusWidget unitStatusWidget = Object.Instantiate<UnitStatusWidget>(this.statusTemplate, this.mainLayer);
			unitStatusWidget.Unit = unit;
			return unitStatusWidget;
		}

		// Token: 0x06000B8D RID: 2957 RVA: 0x0003C1F4 File Offset: 0x0003A3F4
		public UnitInfoWidget CreateInfoWidget(Unit unit)
		{
			UnitInfoWidget unitInfoWidget = Object.Instantiate<UnitInfoWidget>(this.unitInfoTemplate, this.mainLayer);
			unitInfoWidget.Unit = unit;
			return unitInfoWidget;
		}

		// Token: 0x06000B8E RID: 2958 RVA: 0x0003C20E File Offset: 0x0003A40E
		public DollInfoWidget CreateDollWidget(Doll doll)
		{
			DollInfoWidget dollInfoWidget = Object.Instantiate<DollInfoWidget>(this.dollInfoTemplate, this.mainLayer);
			dollInfoWidget.Doll = doll;
			return dollInfoWidget;
		}

		// Token: 0x06000B8F RID: 2959 RVA: 0x0003C228 File Offset: 0x0003A428
		public ChatWidget CreateChatWidget(UnitView unitView)
		{
			ChatWidget chatWidget = Object.Instantiate<ChatWidget>(this.chatTemplate, this.chatLayer);
			chatWidget.TargetTransform = unitView.ChatPoint;
			return chatWidget;
		}

		// Token: 0x06000B90 RID: 2960 RVA: 0x0003C247 File Offset: 0x0003A447
		public void StatusToFront(Transform widget)
		{
			widget.SetParent(base.transform);
			widget.SetParent(this.mainLayer);
		}

		// Token: 0x04000908 RID: 2312
		[SerializeField]
		private Transform mainLayer;

		// Token: 0x04000909 RID: 2313
		[SerializeField]
		private Transform chatLayer;

		// Token: 0x0400090A RID: 2314
		[SerializeField]
		private UnitStatusWidget statusTemplate;

		// Token: 0x0400090B RID: 2315
		[SerializeField]
		private UnitInfoWidget unitInfoTemplate;

		// Token: 0x0400090C RID: 2316
		[SerializeField]
		private DollInfoWidget dollInfoTemplate;

		// Token: 0x0400090D RID: 2317
		[SerializeField]
		private ChatWidget chatTemplate;
	}
}
