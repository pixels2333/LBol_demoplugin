using System;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200006F RID: 111
	public class ScrollBarWidget : MonoBehaviour
	{
		// Token: 0x060005DD RID: 1501 RVA: 0x0001966C File Offset: 0x0001786C
		private void Awake()
		{
			Scrollbar scrollbar = base.GetComponent<Scrollbar>();
			this.buttonUp.onClick.AddListener(delegate
			{
				scrollbar.value += 0.1f;
				if (scrollbar.value > 1f)
				{
					scrollbar.value = 1f;
				}
			});
			this.buttonDown.onClick.AddListener(delegate
			{
				scrollbar.value -= 0.1f;
				if (scrollbar.value < 0f)
				{
					scrollbar.value = 0f;
				}
			});
		}

		// Token: 0x0400038F RID: 911
		[SerializeField]
		private Button buttonUp;

		// Token: 0x04000390 RID: 912
		[SerializeField]
		private Button buttonDown;
	}
}
