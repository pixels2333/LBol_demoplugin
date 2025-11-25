using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.ConfigData;
using LBoL.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class AchievementWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		public string Id { get; private set; }
		public AchievementConfig Config { get; private set; }
		public bool Unlock { get; private set; }
		public bool Reveal { get; private set; }
		public bool Hidden
		{
			get
			{
				return this.Config.Hidden && !this.Unlock && !this.Reveal;
			}
		}
		public IDisplayWord DisplayWord
		{
			get
			{
				return Achievements.GetAchievementDisplayWord(this.Id);
			}
		}
		public void SetAchievement(AchievementConfig config, bool unlock, bool reveal = false)
		{
			this.Id = config.Id;
			this.Unlock = unlock;
			this.Config = config;
			this.Reveal = reveal;
			base.name = this.Id + "(Achievement)";
			Sprite sprite = ResourcesHelper.TryGetAchievementSprite(this.Id);
			if (sprite)
			{
				this.image.sprite = sprite;
			}
			this.lockImage.gameObject.SetActive(!this.Unlock);
			if (this.Hidden)
			{
				this.lockImage.color = Color.white;
			}
		}
		public void OnPointerEnter(PointerEventData eventData)
		{
			base.transform.DOKill(true);
			base.transform.DOScale(1.2f, 0.2f).SetUpdate(true);
			AudioManager.Button(2);
		}
		public void OnPointerExit(PointerEventData eventData)
		{
			base.transform.DOKill(true);
			base.transform.DOScale(1f, 0.2f).SetUpdate(true);
		}
		[SerializeField]
		private RectTransform root;
		[SerializeField]
		private Image image;
		[SerializeField]
		private Image lockImage;
	}
}
