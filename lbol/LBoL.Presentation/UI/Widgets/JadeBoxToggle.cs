using System;
using LBoL.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200005D RID: 93
	public class JadeBoxToggle : MonoBehaviour
	{
		// Token: 0x170000DF RID: 223
		// (get) Token: 0x06000536 RID: 1334 RVA: 0x00016561 File Offset: 0x00014761
		public bool IsOn
		{
			get
			{
				return this.toggle.isOn;
			}
		}

		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x06000537 RID: 1335 RVA: 0x0001656E File Offset: 0x0001476E
		public Toggle Toggle
		{
			get
			{
				return this.toggle;
			}
		}

		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x06000538 RID: 1336 RVA: 0x00016576 File Offset: 0x00014776
		// (set) Token: 0x06000539 RID: 1337 RVA: 0x0001657E File Offset: 0x0001477E
		public JadeBox JadeBox
		{
			get
			{
				return this._jadeBox;
			}
			set
			{
				if (this._jadeBox == value)
				{
					return;
				}
				this._jadeBox = value;
				this.Refresh();
			}
		}

		// Token: 0x0600053A RID: 1338 RVA: 0x00016598 File Offset: 0x00014798
		public void Refresh()
		{
			if (this._jadeBox == null)
			{
				base.gameObject.SetActive(false);
				return;
			}
			this.title.text = this._jadeBox.Name;
			this.description.text = this._jadeBox.Description;
		}

		// Token: 0x040002F7 RID: 759
		[SerializeField]
		private TextMeshProUGUI title;

		// Token: 0x040002F8 RID: 760
		[SerializeField]
		private TextMeshProUGUI description;

		// Token: 0x040002F9 RID: 761
		[SerializeField]
		private Toggle toggle;

		// Token: 0x040002FA RID: 762
		[SerializeField]
		public Image icon;

		// Token: 0x040002FB RID: 763
		[SerializeField]
		public Image bg;

		// Token: 0x040002FC RID: 764
		private JadeBox _jadeBox;
	}
}
