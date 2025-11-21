using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;
using UnityEngine;

namespace LBoL.Presentation.Effect
{
	// Token: 0x02000105 RID: 261
	public class MarisaCharging : MonoBehaviour
	{
		// Token: 0x06000E6C RID: 3692 RVA: 0x00044F55 File Offset: 0x00043155
		[UsedImplicitly]
		public void OnPropertyChanged(StatusEffect effect)
		{
			this.StartType(effect.Level);
		}

		// Token: 0x06000E6D RID: 3693 RVA: 0x00044F64 File Offset: 0x00043164
		private void StartType(int type)
		{
			for (int i = 0; i < this.particleList.Count; i++)
			{
				this.particleList[i].SetActive(i < type);
			}
		}

		// Token: 0x04000ACF RID: 2767
		[SerializeField]
		private List<GameObject> particleList;
	}
}
