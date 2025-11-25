using System;
using TMPro;
using UnityEngine;
namespace LBoL.Presentation.UI.Widgets
{
	public class ScoreWidget : MonoBehaviour
	{
		public void SetContent(string title, int num, int score)
		{
			if (num == 0)
			{
				this.titleText.text = title;
				this.scoreText.text = score.ToString();
				return;
			}
			this.titleText.text = title + string.Format("({0})", num);
			this.scoreText.text = score.ToString();
		}
		[SerializeField]
		private TextMeshProUGUI titleText;
		[SerializeField]
		private TextMeshProUGUI scoreText;
	}
}
