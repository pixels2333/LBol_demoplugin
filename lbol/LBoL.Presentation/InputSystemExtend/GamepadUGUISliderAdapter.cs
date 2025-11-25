using System;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.InputSystemExtend
{
	public class GamepadUGUISliderAdapter : MonoBehaviour
	{
		private void Awake()
		{
			this.slider = base.GetComponent<Slider>();
		}
		public void SetValueByStep(float step)
		{
			this.slider.value = this.slider.value + step;
		}
		private Slider slider;
	}
}
