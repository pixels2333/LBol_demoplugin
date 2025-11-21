using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000043 RID: 67
	public class CardPackWidget : MonoBehaviour
	{
		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x0600042A RID: 1066 RVA: 0x00010BAE File Offset: 0x0000EDAE
		public bool IsLock
		{
			get
			{
				return this._isLock;
			}
		}

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x0600042B RID: 1067 RVA: 0x00010BB6 File Offset: 0x0000EDB6
		public Button Button
		{
			get
			{
				return this.button;
			}
		}

		// Token: 0x0600042C RID: 1068 RVA: 0x00010BC0 File Offset: 0x0000EDC0
		public void Unlock(bool instant = true)
		{
			this._isLock = false;
			if (instant)
			{
				this.lockMask.gameObject.SetActive(false);
				return;
			}
			this.lockMask.DOFade(0f, 0.6f).From(1f, true, false).OnComplete(delegate
			{
				this.lockMask.gameObject.SetActive(false);
			});
		}

		// Token: 0x040001F9 RID: 505
		[SerializeField]
		private Image lockMask;

		// Token: 0x040001FA RID: 506
		[SerializeField]
		private ParticleSystem unlockParticle;

		// Token: 0x040001FB RID: 507
		[SerializeField]
		private Button button;

		// Token: 0x040001FC RID: 508
		private bool _isLock = true;
	}
}
