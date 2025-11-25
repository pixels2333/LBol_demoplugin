using System;
using UnityEngine;
namespace LBoL.Presentation.UI
{
	public abstract class UiTransition : MonoBehaviour
	{
		public abstract void Animate(Transform target, bool isOut, Action onComplete);
		public abstract void Kill(Transform target);
	}
}
