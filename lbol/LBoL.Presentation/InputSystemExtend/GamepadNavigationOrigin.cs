using System;
using UnityEngine;
namespace LBoL.Presentation.InputSystemExtend
{
	public class GamepadNavigationOrigin : MonoBehaviour
	{
		private void OnEnable()
		{
			GamepadNavigationManager.AddOrigin(this);
		}
		private void OnDisable()
		{
			GamepadNavigationManager.RemoveOrigin(this);
		}
	}
}
