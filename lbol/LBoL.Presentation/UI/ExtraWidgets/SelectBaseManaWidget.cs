using System;
using LBoL.Base;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.ExtraWidgets
{
	// Token: 0x020000CF RID: 207
	public class SelectBaseManaWidget : CommonButtonWidget
	{
		// Token: 0x06000C8C RID: 3212 RVA: 0x0003F59C File Offset: 0x0003D79C
		private void Awake()
		{
			this.particleRoot.SetActive(false);
		}

		// Token: 0x06000C8D RID: 3213 RVA: 0x0003F5AC File Offset: 0x0003D7AC
		public void SetSprite(Sprite sprite, ParticleSystem.MinMaxGradient color)
		{
			this.image.sprite = sprite;
			this.particle.main.startColor = color;
		}

		// Token: 0x17000202 RID: 514
		// (get) Token: 0x06000C8E RID: 3214 RVA: 0x0003F5D9 File Offset: 0x0003D7D9
		// (set) Token: 0x06000C8F RID: 3215 RVA: 0x0003F5E1 File Offset: 0x0003D7E1
		public ManaColor Color { get; set; }

		// Token: 0x1400000A RID: 10
		// (add) Token: 0x06000C90 RID: 3216 RVA: 0x0003F5EC File Offset: 0x0003D7EC
		// (remove) Token: 0x06000C91 RID: 3217 RVA: 0x0003F624 File Offset: 0x0003D824
		public event EventHandler SelectedChanged;

		// Token: 0x17000203 RID: 515
		// (get) Token: 0x06000C92 RID: 3218 RVA: 0x0003F659 File Offset: 0x0003D859
		// (set) Token: 0x06000C93 RID: 3219 RVA: 0x0003F661 File Offset: 0x0003D861
		public bool IsSelected { get; private set; }

		// Token: 0x06000C94 RID: 3220 RVA: 0x0003F66A File Offset: 0x0003D86A
		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				this.SetSelected(!this.IsSelected);
				AudioManager.Button(0);
			}
		}

		// Token: 0x06000C95 RID: 3221 RVA: 0x0003F690 File Offset: 0x0003D890
		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);
			AudioManager.Button(2);
		}

		// Token: 0x06000C96 RID: 3222 RVA: 0x0003F6A0 File Offset: 0x0003D8A0
		public void SetSelected(bool isSelect)
		{
			this.IsSelected = isSelect;
			if (this.IsSelected)
			{
				this.image.transform.localScale = Vector3.one * 1.1f;
			}
			else
			{
				this.image.transform.localScale = Vector3.one;
			}
			this.particleRoot.SetActive(this.IsSelected);
			EventHandler selectedChanged = this.SelectedChanged;
			if (selectedChanged == null)
			{
				return;
			}
			selectedChanged.Invoke(this, EventArgs.Empty);
		}

		// Token: 0x0400099F RID: 2463
		[FormerlySerializedAs("selectParticle")]
		[SerializeField]
		private GameObject particleRoot;

		// Token: 0x040009A0 RID: 2464
		[SerializeField]
		private ParticleSystem particle;

		// Token: 0x040009A1 RID: 2465
		[SerializeField]
		private Image image;
	}
}
