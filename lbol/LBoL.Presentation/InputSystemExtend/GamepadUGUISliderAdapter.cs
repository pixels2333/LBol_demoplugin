using System;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.InputSystemExtend
{
	// Token: 0x020000EE RID: 238
	public class GamepadUGUISliderAdapter : MonoBehaviour
	{
		// Token: 0x06000DBA RID: 3514 RVA: 0x000422C4 File Offset: 0x000404C4
		private void Awake()
		{
			this.slider = base.GetComponent<Slider>();
		}

		// Token: 0x06000DBB RID: 3515 RVA: 0x000422D2 File Offset: 0x000404D2
		public void SetValueByStep(float step)
		{
			this.slider.value = this.slider.value + step;
		}

		// Token: 0x04000A54 RID: 2644
		private Slider slider;
	}
}
