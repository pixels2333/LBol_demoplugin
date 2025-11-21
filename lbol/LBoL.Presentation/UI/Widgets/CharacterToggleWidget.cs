using System;
using LBoL.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200004A RID: 74
	public class CharacterToggleWidget : CommonButtonWidget
	{
		// Token: 0x0600048B RID: 1163 RVA: 0x00012A8C File Offset: 0x00010C8C
		public void SetCharacter(string characterName, Sprite sprite, bool isSelectable)
		{
			this.image.sprite = sprite;
			if (isSelectable)
			{
				this.image.color = Color.white;
				this.bg.sprite = this.normalSprite;
				this.text.text = characterName;
			}
			else
			{
				this.image.color = Color.black;
				this.bg.sprite = this.disableSprite;
				this.text.text = "Museum.LockName".Localize(true);
			}
			base.GetComponent<Toggle>().interactable = isSelectable;
		}

		// Token: 0x04000259 RID: 601
		[SerializeField]
		private TextMeshProUGUI text;

		// Token: 0x0400025A RID: 602
		[SerializeField]
		private Image image;

		// Token: 0x0400025B RID: 603
		[FormerlySerializedAs("Bg")]
		[SerializeField]
		private Image bg;

		// Token: 0x0400025C RID: 604
		[SerializeField]
		private Sprite normalSprite;

		// Token: 0x0400025D RID: 605
		[SerializeField]
		private Sprite disableSprite;
	}
}
