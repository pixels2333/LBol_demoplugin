using System;
using LBoL.Presentation.UI.Panels;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000079 RID: 121
	public abstract class TooltipSource : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		// Token: 0x17000110 RID: 272
		// (get) Token: 0x06000631 RID: 1585
		public abstract RectTransform TargetRectTransform { get; }

		// Token: 0x17000111 RID: 273
		// (get) Token: 0x06000632 RID: 1586
		public abstract TooltipPosition[] TooltipPositions { get; }

		// Token: 0x17000112 RID: 274
		// (get) Token: 0x06000633 RID: 1587
		public abstract string Title { get; }

		// Token: 0x17000113 RID: 275
		// (get) Token: 0x06000634 RID: 1588
		public abstract string Description { get; }

		// Token: 0x17000114 RID: 276
		// (get) Token: 0x06000635 RID: 1589 RVA: 0x0001AD4A File Offset: 0x00018F4A
		public virtual float Gap { get; } = 10f;

		// Token: 0x06000636 RID: 1590 RVA: 0x0001AD52 File Offset: 0x00018F52
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.Show();
		}

		// Token: 0x06000637 RID: 1591 RVA: 0x0001AD5A File Offset: 0x00018F5A
		public void OnPointerExit(PointerEventData eventData)
		{
			this.Hide();
		}

		// Token: 0x06000638 RID: 1592 RVA: 0x0001AD62 File Offset: 0x00018F62
		private void OnDisable()
		{
			this.Hide();
		}

		// Token: 0x06000639 RID: 1593 RVA: 0x0001AD6A File Offset: 0x00018F6A
		public void Refresh()
		{
			if (this._visible)
			{
				this.Hide();
				this.Show();
			}
		}

		// Token: 0x0600063A RID: 1594 RVA: 0x0001AD80 File Offset: 0x00018F80
		protected virtual void Show()
		{
			this._visible = true;
			this._id = TooltipsLayer.ShowNormal(this);
		}

		// Token: 0x0600063B RID: 1595 RVA: 0x0001AD95 File Offset: 0x00018F95
		protected virtual void Hide()
		{
			this._visible = false;
			if (this._id != 0)
			{
				TooltipsLayer.Hide(this._id);
				this._id = 0;
			}
		}

		// Token: 0x0600063C RID: 1596 RVA: 0x0001ADB8 File Offset: 0x00018FB8
		public void OnGamepadSelectedChanged(bool value)
		{
			if (value && base.enabled && base.gameObject.activeSelf)
			{
				this.Show();
				return;
			}
			this.Hide();
		}

		// Token: 0x040003D6 RID: 982
		protected int _id;

		// Token: 0x040003D7 RID: 983
		private bool _visible;
	}
}
