using System;
using LBoL.Presentation.UI.Panels;
using UnityEngine;
using UnityEngine.EventSystems;
namespace LBoL.Presentation.UI.Widgets
{
	public abstract class TooltipSource : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		public abstract RectTransform TargetRectTransform { get; }
		public abstract TooltipPosition[] TooltipPositions { get; }
		public abstract string Title { get; }
		public abstract string Description { get; }
		public virtual float Gap { get; } = 10f;
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.Show();
		}
		public void OnPointerExit(PointerEventData eventData)
		{
			this.Hide();
		}
		private void OnDisable()
		{
			this.Hide();
		}
		public void Refresh()
		{
			if (this._visible)
			{
				this.Hide();
				this.Show();
			}
		}
		protected virtual void Show()
		{
			this._visible = true;
			this._id = TooltipsLayer.ShowNormal(this);
		}
		protected virtual void Hide()
		{
			this._visible = false;
			if (this._id != 0)
			{
				TooltipsLayer.Hide(this._id);
				this._id = 0;
			}
		}
		public void OnGamepadSelectedChanged(bool value)
		{
			if (value && base.enabled && base.gameObject.activeSelf)
			{
				this.Show();
				return;
			}
			this.Hide();
		}
		protected int _id;
		private bool _visible;
	}
}
