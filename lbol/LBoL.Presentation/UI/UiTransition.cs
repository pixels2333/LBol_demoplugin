using System;
using UnityEngine;

namespace LBoL.Presentation.UI
{
	// Token: 0x02000038 RID: 56
	public abstract class UiTransition : MonoBehaviour
	{
		// Token: 0x060003C6 RID: 966
		public abstract void Animate(Transform target, bool isOut, Action onComplete);

		// Token: 0x060003C7 RID: 967
		public abstract void Kill(Transform target);
	}
}
