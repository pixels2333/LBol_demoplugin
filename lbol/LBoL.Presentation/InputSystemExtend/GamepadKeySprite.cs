using System;
using TMPro;
using UnityEngine;
namespace LBoL.Presentation.InputSystemExtend
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	[ExecuteAlways]
	public class GamepadKeySprite : MonoBehaviour
	{
		private void Awake()
		{
			this.text = base.GetComponent<TextMeshProUGUI>();
		}
		private void Update()
		{
			if (this.text.text == string.Format("<sprite=\"XboxInput\" name=\"{0}\">", this.key))
			{
				return;
			}
			if (this.text != null)
			{
				this.text.alignment = TextAlignmentOptions.Center;
				this.text.text = string.Format("<sprite=\"XboxInput\" name=\"{0}\">", this.key);
			}
		}
		[SerializeField]
		private GamepadButtonKey key;
		private TextMeshProUGUI text;
	}
}
