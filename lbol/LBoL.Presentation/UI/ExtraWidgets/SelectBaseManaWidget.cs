using System;
using LBoL.Base;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.ExtraWidgets
{
	public class SelectBaseManaWidget : CommonButtonWidget
	{
		private void Awake()
		{
			this.particleRoot.SetActive(false);
		}
		public void SetSprite(Sprite sprite, ParticleSystem.MinMaxGradient color)
		{
			this.image.sprite = sprite;
			this.particle.main.startColor = color;
		}
		public ManaColor Color { get; set; }
		public event EventHandler SelectedChanged;
		public bool IsSelected { get; private set; }
		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				this.SetSelected(!this.IsSelected);
				AudioManager.Button(0);
			}
		}
		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);
			AudioManager.Button(2);
		}
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
		[FormerlySerializedAs("selectParticle")]
		[SerializeField]
		private GameObject particleRoot;
		[SerializeField]
		private ParticleSystem particle;
		[SerializeField]
		private Image image;
	}
}
