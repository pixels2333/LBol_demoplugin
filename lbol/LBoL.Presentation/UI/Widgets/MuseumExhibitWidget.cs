using System;
using LBoL.Base;
using LBoL.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000067 RID: 103
	public class MuseumExhibitWidget : MonoBehaviour
	{
		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x0600058D RID: 1421 RVA: 0x000182AF File Offset: 0x000164AF
		// (set) Token: 0x0600058E RID: 1422 RVA: 0x000182B7 File Offset: 0x000164B7
		public bool IsReveal { get; set; } = true;

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x0600058F RID: 1423 RVA: 0x000182C0 File Offset: 0x000164C0
		// (set) Token: 0x06000590 RID: 1424 RVA: 0x000182C8 File Offset: 0x000164C8
		public bool IsLock { get; set; }

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x06000591 RID: 1425 RVA: 0x000182D1 File Offset: 0x000164D1
		// (set) Token: 0x06000592 RID: 1426 RVA: 0x000182DE File Offset: 0x000164DE
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

		// Token: 0x06000593 RID: 1427 RVA: 0x00018314 File Offset: 0x00016514
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

		// Token: 0x06000594 RID: 1428 RVA: 0x000183A2 File Offset: 0x000165A2
		public void OpenTooltip()
		{
			this.exhibitWidget.GetComponent<TooltipSource>().enabled = true;
		}

		// Token: 0x06000595 RID: 1429 RVA: 0x000183B5 File Offset: 0x000165B5
		private void Awake()
		{
			this.exhibitWidget.ShowCounter = false;
		}

		// Token: 0x0400034C RID: 844
		public ExhibitWidget exhibitWidget;

		// Token: 0x0400034D RID: 845
		[SerializeField]
		private Image background;

		// Token: 0x0400034E RID: 846
		[SerializeField]
		private Image lockMask;

		// Token: 0x0400034F RID: 847
		[SerializeField]
		private AssociationList<Rarity, Sprite> spriteList;
	}
}
