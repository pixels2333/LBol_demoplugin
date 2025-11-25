using System;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.EventSystems;
namespace LBoL.Presentation.UI.ExtraWidgets
{
	[RequireComponent(typeof(CardWidget))]
	public class SelectCardWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
	{
		public CardWidget CardWidget { get; private set; }
		private void Awake()
		{
			this.CardWidget = base.GetComponent<CardWidget>();
		}
		public Card Card
		{
			get
			{
				return this.CardWidget.Card;
			}
		}
		public event EventHandler SelectedChanged;
		public bool IsSelected { get; private set; }
		public GameObject SelectParticle { get; set; }
		public void SetSelected(bool select, bool notify = true)
		{
			this.IsSelected = select;
			this.SelectParticle.SetActive(this.IsSelected);
			if (notify)
			{
				EventHandler selectedChanged = this.SelectedChanged;
				if (selectedChanged == null)
				{
					return;
				}
				selectedChanged.Invoke(this, EventArgs.Empty);
			}
		}
		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				this.IsSelected = !this.IsSelected;
				this.SelectParticle.SetActive(this.IsSelected);
				EventHandler selectedChanged = this.SelectedChanged;
				if (selectedChanged == null)
				{
					return;
				}
				selectedChanged.Invoke(this, EventArgs.Empty);
			}
		}
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.CardWidget.ShowTooltip();
		}
		public void OnPointerExit(PointerEventData eventData)
		{
			this.CardWidget.HideTooltip();
		}
	}
}
