using System;
using Cysharp.Threading.Tasks;
using LBoL.Core;
using LBoL.Presentation.InputSystemExtend;
using UnityEngine;
using UnityEngine.Events;

namespace LBoL.Presentation.UI
{
	// Token: 0x0200002E RID: 46
	public abstract class UiBase : MonoBehaviour
	{
		// Token: 0x1700008B RID: 139
		// (get) Token: 0x06000348 RID: 840 RVA: 0x0000E5E8 File Offset: 0x0000C7E8
		// (set) Token: 0x06000349 RID: 841 RVA: 0x0000E5F0 File Offset: 0x0000C7F0
		public bool IsVisible { get; protected set; }

		// Token: 0x0600034A RID: 842 RVA: 0x0000E5F9 File Offset: 0x0000C7F9
		public virtual UniTask InitializeAsync()
		{
			return UniTask.CompletedTask;
		}

		// Token: 0x0600034B RID: 843 RVA: 0x0000E600 File Offset: 0x0000C800
		public virtual UniTask CustomLocalizationAsync()
		{
			this.OnLocaleChanged();
			return UniTask.CompletedTask;
		}

		// Token: 0x0600034C RID: 844 RVA: 0x0000E60D File Offset: 0x0000C80D
		public virtual void OnLocaleChanged()
		{
		}

		// Token: 0x0600034D RID: 845 RVA: 0x0000E60F File Offset: 0x0000C80F
		protected virtual void OnShown()
		{
		}

		// Token: 0x0600034E RID: 846 RVA: 0x0000E611 File Offset: 0x0000C811
		protected virtual void OnHiding()
		{
		}

		// Token: 0x0600034F RID: 847 RVA: 0x0000E613 File Offset: 0x0000C813
		protected virtual void OnHided()
		{
		}

		// Token: 0x06000350 RID: 848 RVA: 0x0000E615 File Offset: 0x0000C815
		protected void DoTransitionIn()
		{
			if (this.transitionIn)
			{
				this._isTransitionIn = true;
				this.transitionIn.Animate(base.transform, false, new Action(this.OnTransitionInFinished));
				return;
			}
			this.OnTransitionInFinished();
		}

		// Token: 0x06000351 RID: 849 RVA: 0x0000E651 File Offset: 0x0000C851
		protected void KillTransitionIn()
		{
			if (this._isTransitionIn && this.transitionIn)
			{
				this.transitionIn.Kill(base.transform);
				this._isTransitionIn = false;
			}
		}

		// Token: 0x06000352 RID: 850 RVA: 0x0000E680 File Offset: 0x0000C880
		protected void DoTransitionOut()
		{
			if (this.transitionOut)
			{
				this._isTransitionOut = true;
				this.transitionOut.Animate(base.transform, !this.dontReverseOut, new Action(this.OnTransitionOutFinished));
				return;
			}
			this.OnTransitionOutFinished();
		}

		// Token: 0x06000353 RID: 851 RVA: 0x0000E6CF File Offset: 0x0000C8CF
		protected void KillTransitionOut()
		{
			if (this._isTransitionOut && this.transitionOut)
			{
				this.transitionOut.Kill(base.transform);
				this._isTransitionOut = false;
			}
		}

		// Token: 0x06000354 RID: 852 RVA: 0x0000E6FE File Offset: 0x0000C8FE
		protected virtual void OnTransitionInFinished()
		{
			this._isTransitionIn = false;
			this.OnShown();
			Singleton<InputDeviceManager>.Instance.OnInputDeviceChanged.AddListener(new UnityAction<InputDeviceType>(this.OnInputDeviceChanged));
			this.OnInputDeviceChanged(Singleton<InputDeviceManager>.Instance.CurrentInputDevice);
		}

		// Token: 0x06000355 RID: 853 RVA: 0x0000E73C File Offset: 0x0000C93C
		protected virtual void OnTransitionOutFinished()
		{
			this._isTransitionOut = false;
			if (base.gameObject)
			{
				base.gameObject.SetActive(false);
			}
			this.OnHided();
			InputDeviceManager instance = Singleton<InputDeviceManager>.Instance;
			if (instance == null)
			{
				return;
			}
			instance.OnInputDeviceChanged.RemoveListener(new UnityAction<InputDeviceType>(this.OnInputDeviceChanged));
		}

		// Token: 0x06000356 RID: 854 RVA: 0x0000E790 File Offset: 0x0000C990
		public virtual void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
		}

		// Token: 0x04000180 RID: 384
		[SerializeField]
		private UiTransition transitionIn;

		// Token: 0x04000181 RID: 385
		[SerializeField]
		private UiTransition transitionOut;

		// Token: 0x04000182 RID: 386
		[SerializeField]
		private bool dontReverseOut;

		// Token: 0x04000183 RID: 387
		[SerializeField]
		protected bool enableGamepadInputInTransitionIn;

		// Token: 0x04000184 RID: 388
		private bool _isTransitionIn;

		// Token: 0x04000185 RID: 389
		private bool _isTransitionOut;
	}
}
