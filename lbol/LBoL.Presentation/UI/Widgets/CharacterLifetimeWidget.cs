using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000049 RID: 73
	public class CharacterLifetimeWidget : MonoBehaviour
	{
		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x06000485 RID: 1157 RVA: 0x00012A06 File Offset: 0x00010C06
		// (set) Token: 0x06000486 RID: 1158 RVA: 0x00012A0E File Offset: 0x00010C0E
		public int Order { get; set; } = -99;

		// Token: 0x06000487 RID: 1159 RVA: 0x00012A17 File Offset: 0x00010C17
		public void SetValue(string valueText)
		{
			this.valueTmp.text = valueText;
		}

		// Token: 0x06000488 RID: 1160 RVA: 0x00012A25 File Offset: 0x00010C25
		public void SetTitle([CanBeNull] PlayerUnit chara, bool isOld)
		{
			this.Chara = chara;
			this.isOld = isOld;
			this.Refresh();
		}

		// Token: 0x06000489 RID: 1161 RVA: 0x00012A3B File Offset: 0x00010C3B
		public void Refresh()
		{
			if (this.isOld)
			{
				this.nameTmp.text = "Lifetime.OldData".Localize(true);
				return;
			}
			if (this.Chara != null)
			{
				this.nameTmp.text = this.Chara.Name;
			}
		}

		// Token: 0x04000254 RID: 596
		[SerializeField]
		private TextMeshProUGUI nameTmp;

		// Token: 0x04000255 RID: 597
		[SerializeField]
		private TextMeshProUGUI valueTmp;

		// Token: 0x04000257 RID: 599
		public PlayerUnit Chara;

		// Token: 0x04000258 RID: 600
		public bool isOld;
	}
}
