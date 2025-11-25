using System;
using LBoL.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class AchievementHintWidget : MonoBehaviour
	{
		public void SetAchievementHint(string key)
		{
			IDisplayWord achievementDisplayWord = Achievements.GetAchievementDisplayWord(key);
			this.title.text = achievementDisplayWord.Name;
			this.description.text = achievementDisplayWord.Description;
			Sprite sprite = ResourcesHelper.TryGetAchievementSprite(key);
			if (sprite != null)
			{
				this.image.sprite = sprite;
			}
		}
		[SerializeField]
		private RectTransform root;
		[SerializeField]
		private Image image;
		[SerializeField]
		private TextMeshProUGUI title;
		[SerializeField]
		private TextMeshProUGUI description;
	}
}
