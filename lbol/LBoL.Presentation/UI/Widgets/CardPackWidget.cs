using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class CardPackWidget : MonoBehaviour
	{
		public bool IsLock
		{
			get
			{
				return this._isLock;
			}
		}
		public Button Button
		{
			get
			{
				return this.button;
			}
		}
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
		[SerializeField]
		private Image lockMask;
		[SerializeField]
		private ParticleSystem unlockParticle;
		[SerializeField]
		private Button button;
		private bool _isLock = true;
	}
}
