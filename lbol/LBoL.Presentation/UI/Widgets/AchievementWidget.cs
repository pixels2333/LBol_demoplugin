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
	// Token: 0x0200003A RID: 58
	public class AchievementWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		// Token: 0x17000098 RID: 152
		// (get) Token: 0x060003CB RID: 971 RVA: 0x0000FCCA File Offset: 0x0000DECA
		// (set) Token: 0x060003CC RID: 972 RVA: 0x0000FCD2 File Offset: 0x0000DED2
		public string Id { get; private set; }

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x060003CD RID: 973 RVA: 0x0000FCDB File Offset: 0x0000DEDB
		// (set) Token: 0x060003CE RID: 974 RVA: 0x0000FCE3 File Offset: 0x0000DEE3
		public AchievementConfig Config { get; private set; }

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x060003CF RID: 975 RVA: 0x0000FCEC File Offset: 0x0000DEEC
		// (set) Token: 0x060003D0 RID: 976 RVA: 0x0000FCF4 File Offset: 0x0000DEF4
		public bool Unlock { get; private set; }

		// Token: 0x1700009B RID: 155
		// (get) Token: 0x060003D1 RID: 977 RVA: 0x0000FCFD File Offset: 0x0000DEFD
		// (set) Token: 0x060003D2 RID: 978 RVA: 0x0000FD05 File Offset: 0x0000DF05
		public bool Reveal { get; private set; }

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x060003D3 RID: 979 RVA: 0x0000FD0E File Offset: 0x0000DF0E
		public bool Hidden
		{
			get
			{
				return this.Config.Hidden && !this.Unlock && !this.Reveal;
			}
		}

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x060003D4 RID: 980 RVA: 0x0000FD30 File Offset: 0x0000DF30
		public IDisplayWord DisplayWord
		{
			get
			{
				return Achievements.GetAchievementDisplayWord(this.Id);
			}
		}

		// Token: 0x060003D5 RID: 981 RVA: 0x0000FD40 File Offset: 0x0000DF40
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

		// Token: 0x060003D6 RID: 982 RVA: 0x0000FDD5 File Offset: 0x0000DFD5
		public void OnPointerEnter(PointerEventData eventData)
		{
			base.transform.DOKill(true);
			base.transform.DOScale(1.2f, 0.2f).SetUpdate(true);
			AudioManager.Button(2);
		}

		// Token: 0x060003D7 RID: 983 RVA: 0x0000FE06 File Offset: 0x0000E006
		public void OnPointerExit(PointerEventData eventData)
		{
			base.transform.DOKill(true);
			base.transform.DOScale(1f, 0.2f).SetUpdate(true);
		}

		// Token: 0x040001C0 RID: 448
		[SerializeField]
		private RectTransform root;

		// Token: 0x040001C1 RID: 449
		[SerializeField]
		private Image image;

		// Token: 0x040001C2 RID: 450
		[SerializeField]
		private Image lockImage;
	}
}
