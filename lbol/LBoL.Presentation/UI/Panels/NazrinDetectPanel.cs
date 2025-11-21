using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Plugins.Options;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.EntityLib.Adventures.Shared23;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000A6 RID: 166
	public class NazrinDetectPanel : UiAdventurePanel<NazrinDetect>
	{
		// Token: 0x1700016E RID: 366
		// (get) Token: 0x060008E6 RID: 2278 RVA: 0x0002D4D2 File Offset: 0x0002B6D2
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}

		// Token: 0x060008E7 RID: 2279 RVA: 0x0002D4D8 File Offset: 0x0002B6D8
		private void Update()
		{
			if (this.currentIndex != (int)this.pointer.eulerAngles.z / 60)
			{
				int num = this.currentIndex;
				this.currentIndex = (int)this.pointer.eulerAngles.z / 60;
				this.cursor.eulerAngles = new Vector3(0f, 0f, (float)(30 + this.currentIndex * 60));
				this.iconList[this.currentIndex].transform.DOScale(1.1f, 0.1f).From(1f, true, false);
				this.iconList[num].transform.DOScale(1f, 0.1f).From(1.1f, true, false);
			}
		}

		// Token: 0x060008E8 RID: 2280 RVA: 0x0002D5AB File Offset: 0x0002B7AB
		public void StartPlate(int finalIndex)
		{
		}

		// Token: 0x060008E9 RID: 2281 RVA: 0x0002D5AD File Offset: 0x0002B7AD
		[RuntimeCommand("showWheel", "")]
		[UsedImplicitly]
		public IEnumerator ShowWheel()
		{
			this.pointer.transform.eulerAngles = Vector3.zero;
			base.Show();
			this.panel.DOFade(1f, 0.2f).From(0f, true, false);
			this.panel.transform.DOScale(1f, 0.5f).From(0.1f, true, false);
			yield return new WaitForSeconds(0.5f);
			yield break;
		}

		// Token: 0x060008EA RID: 2282 RVA: 0x0002D5BC File Offset: 0x0002B7BC
		[RuntimeCommand("hideWheel", "")]
		[UsedImplicitly]
		public IEnumerator HideWheel()
		{
			this.panel.DOFade(0f, 0.5f).From(1f, true, false);
			yield return new WaitForSeconds(0.5f);
			base.Hide();
			yield return new WaitForSeconds(0.5f);
			yield break;
		}

		// Token: 0x060008EB RID: 2283 RVA: 0x0002D5CB File Offset: 0x0002B7CB
		[RuntimeCommand("roll", "")]
		[UsedImplicitly]
		public IEnumerator Roll(int resultIndex)
		{
			this.pointer.transform.DORotate(new Vector3(0f, 0f, (float)(3630 + resultIndex * 60)), 4f, RotateMode.FastBeyond360).From(Vector3.zero, true, false);
			yield return new WaitForSeconds(5f);
			yield break;
		}

		// Token: 0x040006A7 RID: 1703
		[SerializeField]
		private CanvasGroup panel;

		// Token: 0x040006A8 RID: 1704
		[SerializeField]
		private Transform pointer;

		// Token: 0x040006A9 RID: 1705
		[SerializeField]
		private Transform cursor;

		// Token: 0x040006AA RID: 1706
		[SerializeField]
		private List<Image> iconList = new List<Image>();

		// Token: 0x040006AB RID: 1707
		private int currentIndex;
	}
}
