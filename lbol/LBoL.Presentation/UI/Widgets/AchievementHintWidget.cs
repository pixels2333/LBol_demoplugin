using System;
using LBoL.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000039 RID: 57
	public class AchievementHintWidget : MonoBehaviour
	{
		// Token: 0x060003C9 RID: 969 RVA: 0x0000FC70 File Offset: 0x0000DE70
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

		// Token: 0x040001B8 RID: 440
		[SerializeField]
		private RectTransform root;

		// Token: 0x040001B9 RID: 441
		[SerializeField]
		private Image image;

		// Token: 0x040001BA RID: 442
		[SerializeField]
		private TextMeshProUGUI title;

		// Token: 0x040001BB RID: 443
		[SerializeField]
		private TextMeshProUGUI description;
	}
}
