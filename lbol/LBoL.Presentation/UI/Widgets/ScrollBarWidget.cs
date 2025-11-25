using System;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class ScrollBarWidget : MonoBehaviour
	{
		private void Awake()
		{
			Scrollbar scrollbar = base.GetComponent<Scrollbar>();
			this.buttonUp.onClick.AddListener(delegate
			{
				scrollbar.value += 0.1f;
				if (scrollbar.value > 1f)
				{
					scrollbar.value = 1f;
				}
			});
			this.buttonDown.onClick.AddListener(delegate
			{
				scrollbar.value -= 0.1f;
				if (scrollbar.value < 0f)
				{
					scrollbar.value = 0f;
				}
			});
		}
		[SerializeField]
		private Button buttonUp;
		[SerializeField]
		private Button buttonDown;
	}
}
