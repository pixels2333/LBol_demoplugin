using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;
using UnityEngine;

namespace LBoL.Presentation.Effect
{
	// Token: 0x02000107 RID: 263
	public class ReimiChain : MonoBehaviour
	{
		// Token: 0x06000E82 RID: 3714 RVA: 0x0004527C File Offset: 0x0004347C
		[UsedImplicitly]
		public void OnPropertyChanged(StatusEffect effect)
		{
			for (int i = 0; i < this.particleList.Count; i++)
			{
				int limit = effect.Limit;
				this.particleList[i].SetActive(i < 7 - effect.Count);
			}
		}

		// Token: 0x04000ADB RID: 2779
		[SerializeField]
		private List<GameObject> particleList;
	}
}
