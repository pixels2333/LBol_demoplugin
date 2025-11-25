using System;
using LBoL.Core.Units;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using UnityEngine;
namespace LBoL.Presentation.UI.Panels
{
	[AddComponentMenu(null)]
	public class UnitStatusHud : UiPanel
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Bottom;
			}
		}
		public void HideAll()
		{
			this.mainLayer.gameObject.SetActive(false);
			this.chatLayer.gameObject.SetActive(false);
		}
		public void RevealAll()
		{
			this.mainLayer.gameObject.SetActive(true);
			this.chatLayer.gameObject.SetActive(true);
		}
		public UnitStatusWidget CreateStatusWidget(Unit unit)
		{
			UnitStatusWidget unitStatusWidget = Object.Instantiate<UnitStatusWidget>(this.statusTemplate, this.mainLayer);
			unitStatusWidget.Unit = unit;
			return unitStatusWidget;
		}
		public UnitInfoWidget CreateInfoWidget(Unit unit)
		{
			UnitInfoWidget unitInfoWidget = Object.Instantiate<UnitInfoWidget>(this.unitInfoTemplate, this.mainLayer);
			unitInfoWidget.Unit = unit;
			return unitInfoWidget;
		}
		public DollInfoWidget CreateDollWidget(Doll doll)
		{
			DollInfoWidget dollInfoWidget = Object.Instantiate<DollInfoWidget>(this.dollInfoTemplate, this.mainLayer);
			dollInfoWidget.Doll = doll;
			return dollInfoWidget;
		}
		public ChatWidget CreateChatWidget(UnitView unitView)
		{
			ChatWidget chatWidget = Object.Instantiate<ChatWidget>(this.chatTemplate, this.chatLayer);
			chatWidget.TargetTransform = unitView.ChatPoint;
			return chatWidget;
		}
		public void StatusToFront(Transform widget)
		{
			widget.SetParent(base.transform);
			widget.SetParent(this.mainLayer);
		}
		[SerializeField]
		private Transform mainLayer;
		[SerializeField]
		private Transform chatLayer;
		[SerializeField]
		private UnitStatusWidget statusTemplate;
		[SerializeField]
		private UnitInfoWidget unitInfoTemplate;
		[SerializeField]
		private DollInfoWidget dollInfoTemplate;
		[SerializeField]
		private ChatWidget chatTemplate;
	}
}
