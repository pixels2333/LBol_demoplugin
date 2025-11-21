using System;
using UnityEngine;

namespace LBoL.Presentation.InputSystemExtend
{
	// Token: 0x020000EB RID: 235
	public class GamepadNavigationOrigin : MonoBehaviour
	{
		// Token: 0x06000DA6 RID: 3494 RVA: 0x00041C33 File Offset: 0x0003FE33
		private void OnEnable()
		{
			GamepadNavigationManager.AddOrigin(this);
		}

		// Token: 0x06000DA7 RID: 3495 RVA: 0x00041C3B File Offset: 0x0003FE3B
		private void OnDisable()
		{
			GamepadNavigationManager.RemoveOrigin(this);
		}
	}
}
