using System;
using Cysharp.Threading.Tasks;
using LBoL.Core;
using LBoL.Presentation.InputSystemExtend;
using UnityEngine;
using UnityEngine.Events;
namespace LBoL.Presentation.UI
{
	public abstract class UiBase : MonoBehaviour
	{
		public bool IsVisible { get; protected set; }
		public virtual UniTask InitializeAsync()
		{
			return UniTask.CompletedTask;
		}
		public virtual UniTask CustomLocalizationAsync()
		{
			this.OnLocaleChanged();
			return UniTask.CompletedTask;
		}
		public virtual void OnLocaleChanged()
		{
		}
		protected virtual void OnShown()
		{
		}
		protected virtual void OnHiding()
		{
		}
		protected virtual void OnHided()
		{
		}
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
		protected void KillTransitionIn()
		{
			if (this._isTransitionIn && this.transitionIn)
			{
				this.transitionIn.Kill(base.transform);
				this._isTransitionIn = false;
			}
		}
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
		protected void KillTransitionOut()
		{
			if (this._isTransitionOut && this.transitionOut)
			{
				this.transitionOut.Kill(base.transform);
				this._isTransitionOut = false;
			}
		}
		protected virtual void OnTransitionInFinished()
		{
			this._isTransitionIn = false;
			this.OnShown();
			Singleton<InputDeviceManager>.Instance.OnInputDeviceChanged.AddListener(new UnityAction<InputDeviceType>(this.OnInputDeviceChanged));
			this.OnInputDeviceChanged(Singleton<InputDeviceManager>.Instance.CurrentInputDevice);
		}
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
		public virtual void OnInputDeviceChanged(InputDeviceType inputDevice)
		{
		}
		[SerializeField]
		private UiTransition transitionIn;
		[SerializeField]
		private UiTransition transitionOut;
		[SerializeField]
		private bool dontReverseOut;
		[SerializeField]
		protected bool enableGamepadInputInTransitionIn;
		private bool _isTransitionIn;
		private bool _isTransitionOut;
	}
}
