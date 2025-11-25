using System;
using LBoL.Base;
using LBoL.Core;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class MuseumExhibitWidget : MonoBehaviour
	{
		public bool IsReveal { get; set; } = true;
		public bool IsLock { get; set; }
		public Exhibit Exhibit
		{
			get
			{
				return this.exhibitWidget.Exhibit;
			}
			set
			{
				this.background.sprite = this.spriteList[value.Config.Rarity];
				this.exhibitWidget.Exhibit = value;
				this.Refresh();
			}
		}
		public void Refresh()
		{
			if (this.lockMask != null)
			{
				this.lockMask.gameObject.SetActive(this.IsLock);
				if (this.IsLock || !this.IsReveal)
				{
					this.exhibitWidget.MainImage.color = Color.black;
					this.exhibitWidget.GetComponent<TooltipSource>().enabled = false;
					return;
				}
				this.exhibitWidget.MainImage.color = Color.white;
				this.exhibitWidget.GetComponent<TooltipSource>().enabled = true;
			}
		}
		public void OpenTooltip()
		{
			this.exhibitWidget.GetComponent<TooltipSource>().enabled = true;
		}
		private void Awake()
		{
			this.exhibitWidget.ShowCounter = false;
		}
		public ExhibitWidget exhibitWidget;
		[SerializeField]
		private Image background;
		[SerializeField]
		private Image lockMask;
		[SerializeField]
		private AssociationList<Rarity, Sprite> spriteList;
	}
}
