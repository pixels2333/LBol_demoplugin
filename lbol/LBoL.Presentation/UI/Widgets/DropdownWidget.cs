using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000052 RID: 82
	public class DropdownWidget : CommonButtonWidget
	{
		// Token: 0x170000CE RID: 206
		// (get) Token: 0x060004C7 RID: 1223 RVA: 0x00013E56 File Offset: 0x00012056
		// (set) Token: 0x060004C8 RID: 1224 RVA: 0x00013E5E File Offset: 0x0001205E
		private bool IsExpanded
		{
			get
			{
				return this._isExpanded;
			}
			set
			{
				this._isExpanded = value;
				this.OnOpenOrClose(value);
			}
		}

		// Token: 0x060004C9 RID: 1225 RVA: 0x00013E70 File Offset: 0x00012070
		private void OnOpenOrClose(bool isOpen)
		{
			this.cursor.rectTransform.localRotation = Quaternion.Euler(0f, 0f, (float)(this.IsExpanded ? 0 : 180));
			CommonButtonWidget.ButtonWeight buttonWeight = this.buttonWeight;
			if (buttonWeight == CommonButtonWidget.ButtonWeight.Normal)
			{
				AudioManager.Button(isOpen ? 0 : 1);
				return;
			}
			if (buttonWeight != CommonButtonWidget.ButtonWeight.Light)
			{
				throw new ArgumentOutOfRangeException();
			}
			AudioManager.Button(isOpen ? 3 : 4);
		}

		// Token: 0x060004CA RID: 1226 RVA: 0x00013EDC File Offset: 0x000120DC
		public void Update()
		{
			if (this.IsExpanded == this.dropdown.IsExpanded)
			{
				return;
			}
			this.IsExpanded = this.dropdown.IsExpanded;
		}

		// Token: 0x04000290 RID: 656
		[SerializeField]
		private Image cursor;

		// Token: 0x04000291 RID: 657
		public TMP_Dropdown dropdown;

		// Token: 0x04000292 RID: 658
		private bool _isExpanded;
	}
}
