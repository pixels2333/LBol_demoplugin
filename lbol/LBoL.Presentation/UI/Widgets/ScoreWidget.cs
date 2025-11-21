using System;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200006E RID: 110
	public class ScoreWidget : MonoBehaviour
	{
		// Token: 0x060005DB RID: 1499 RVA: 0x00019600 File Offset: 0x00017800
		public void SetContent(string title, int num, int score)
		{
			if (num == 0)
			{
				this.titleText.text = title;
				this.scoreText.text = score.ToString();
				return;
			}
			this.titleText.text = title + string.Format("({0})", num);
			this.scoreText.text = score.ToString();
		}

		// Token: 0x0400038D RID: 909
		[SerializeField]
		private TextMeshProUGUI titleText;

		// Token: 0x0400038E RID: 910
		[SerializeField]
		private TextMeshProUGUI scoreText;
	}
}
