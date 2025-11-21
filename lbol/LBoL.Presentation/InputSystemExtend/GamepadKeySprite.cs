using System;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.InputSystemExtend
{
	// Token: 0x020000E9 RID: 233
	[RequireComponent(typeof(TextMeshProUGUI))]
	[ExecuteAlways]
	public class GamepadKeySprite : MonoBehaviour
	{
		// Token: 0x06000D96 RID: 3478 RVA: 0x000418F8 File Offset: 0x0003FAF8
		private void Awake()
		{
			this.text = base.GetComponent<TextMeshProUGUI>();
		}

		// Token: 0x06000D97 RID: 3479 RVA: 0x00041908 File Offset: 0x0003FB08
		private void Update()
		{
			if (this.text.text == string.Format("<sprite=\"XboxInput\" name=\"{0}\">", this.key))
			{
				return;
			}
			if (this.text != null)
			{
				this.text.alignment = TextAlignmentOptions.Center;
				this.text.text = string.Format("<sprite=\"XboxInput\" name=\"{0}\">", this.key);
			}
		}

		// Token: 0x04000A3E RID: 2622
		[SerializeField]
		private GamepadButtonKey key;

		// Token: 0x04000A3F RID: 2623
		private TextMeshProUGUI text;
	}
}
