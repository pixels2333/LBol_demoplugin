using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Helpers;
using TMPro;
using UnityEngine;
namespace LBoL.Presentation.UI.Widgets
{
	public class MuseumExhibitTooltip : MonoBehaviour
	{
		private void Awake()
		{
			if (this.mainTemplate != null)
			{
				this.mainTemplate.gameObject.SetActive(false);
			}
			if (this.dwTemplate != null)
			{
				this.dwTemplate.gameObject.SetActive(false);
			}
			if (this.flavorTemplate != null)
			{
				this.flavorTemplate.gameObject.SetActive(false);
			}
		}
		private void SetContent(string mainText, IEnumerable<IDisplayWord> displayWords, [CanBeNull] string flvText = null)
		{
			this.root.DestroyChildren();
			RectTransform rectTransform = Object.Instantiate<RectTransform>(this.mainTemplate, this.root);
			TextMeshProUGUI componentInChildren = rectTransform.GetComponentInChildren<TextMeshProUGUI>();
			rectTransform.gameObject.SetActive(true);
			componentInChildren.text = mainText;
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, UiUtils.GetPreferredSize(componentInChildren, rectTransform.sizeDelta.x, float.MaxValue).y);
			if (flvText != null)
			{
				RectTransform rectTransform2 = Object.Instantiate<RectTransform>(this.flavorTemplate, this.root);
				rectTransform2.gameObject.SetActive(true);
				TextMeshProUGUI componentInChildren2 = rectTransform2.GetComponentInChildren<TextMeshProUGUI>();
				componentInChildren2.text = flvText;
				rectTransform2.sizeDelta = new Vector2(rectTransform2.sizeDelta.x, UiUtils.GetPreferredSize(componentInChildren2, rectTransform2.sizeDelta.x, float.MaxValue).y);
			}
			foreach (IDisplayWord displayWord in displayWords)
			{
				RectTransform rectTransform3 = Object.Instantiate<RectTransform>(this.dwTemplate, this.root);
				rectTransform3.gameObject.SetActive(true);
				TextMeshProUGUI textMeshProUGUI = rectTransform3.GetComponentsInChildren<TextMeshProUGUI>()[0];
				TextMeshProUGUI textMeshProUGUI2 = rectTransform3.GetComponentsInChildren<TextMeshProUGUI>()[1];
				textMeshProUGUI.text = displayWord.Name;
				textMeshProUGUI2.text = displayWord.Description;
				rectTransform3.sizeDelta = new Vector2(rectTransform3.sizeDelta.x, UiUtils.GetPreferredSize(textMeshProUGUI, rectTransform3.sizeDelta.x, float.MaxValue).y + UiUtils.GetPreferredSize(textMeshProUGUI2, rectTransform3.sizeDelta.x, float.MaxValue).y);
			}
		}
		public void SetExhibit(Exhibit exhibit)
		{
			this.SetContent(exhibit.Description, exhibit.EnumerateDisplayWords(GameMaster.ShowVerboseKeywords), exhibit.FlavorText);
		}
		[SerializeField]
		private RectTransform root;
		[SerializeField]
		private RectTransform mainTemplate;
		[SerializeField]
		private RectTransform flavorTemplate;
		[SerializeField]
		private RectTransform dwTemplate;
	}
}
