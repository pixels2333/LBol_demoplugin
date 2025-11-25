using System;
using LBoL.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class CharacterToggleWidget : CommonButtonWidget
	{
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
		[SerializeField]
		private TextMeshProUGUI text;
		[SerializeField]
		private Image image;
		[FormerlySerializedAs("Bg")]
		[SerializeField]
		private Image bg;
		[SerializeField]
		private Sprite normalSprite;
		[SerializeField]
		private Sprite disableSprite;
	}
}
