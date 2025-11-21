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
	// Token: 0x02000066 RID: 102
	public class MuseumExhibitTooltip : MonoBehaviour
	{
		// Token: 0x06000589 RID: 1417 RVA: 0x00018060 File Offset: 0x00016260
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

		// Token: 0x0600058A RID: 1418 RVA: 0x000180CC File Offset: 0x000162CC
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

		// Token: 0x0600058B RID: 1419 RVA: 0x00018288 File Offset: 0x00016488
		public void SetExhibit(Exhibit exhibit)
		{
			this.SetContent(exhibit.Description, exhibit.EnumerateDisplayWords(GameMaster.ShowVerboseKeywords), exhibit.FlavorText);
		}

		// Token: 0x04000348 RID: 840
		[SerializeField]
		private RectTransform root;

		// Token: 0x04000349 RID: 841
		[SerializeField]
		private RectTransform mainTemplate;

		// Token: 0x0400034A RID: 842
		[SerializeField]
		private RectTransform flavorTemplate;

		// Token: 0x0400034B RID: 843
		[SerializeField]
		private RectTransform dwTemplate;
	}
}
