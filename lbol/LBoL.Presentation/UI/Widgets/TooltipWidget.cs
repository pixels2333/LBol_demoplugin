using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core.Helpers;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200007A RID: 122
	public class TooltipWidget : MonoBehaviour
	{
		// Token: 0x17000115 RID: 277
		// (get) Token: 0x0600063E RID: 1598 RVA: 0x0001ADF2 File Offset: 0x00018FF2
		// (set) Token: 0x0600063F RID: 1599 RVA: 0x0001ADFA File Offset: 0x00018FFA
		public TooltipSource Source { get; private set; }

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x06000640 RID: 1600 RVA: 0x0001AE03 File Offset: 0x00019003
		private static float MaxWidth
		{
			get
			{
				if (!GameMaster.PreferWideTooltips)
				{
					return 500f;
				}
				return 720f;
			}
		}

		// Token: 0x06000641 RID: 1601 RVA: 0x0001AE17 File Offset: 0x00019017
		private void OnEnable()
		{
			this.canvasGroup.DOFade(1f, 0.1f).From(0f, true, false).SetDelay(0.1f)
				.SetUpdate(true);
		}

		// Token: 0x06000642 RID: 1602 RVA: 0x0001AE4B File Offset: 0x0001904B
		private void OnDestroy()
		{
			this.canvasGroup.DOKill(false);
		}

		// Token: 0x06000643 RID: 1603 RVA: 0x0001AE5C File Offset: 0x0001905C
		private void SetContent(bool fixedWidth, string title, [CanBeNull] string description)
		{
			Vector2 vector = new Vector2(0f, this.titleText.rectTransform.rect.height);
			if (fixedWidth)
			{
				vector.x = TooltipWidget.MaxWidth;
				this.titleText.text = title;
				float num;
				float num2;
				UiUtils.GetPreferredSize(this.titleText, TooltipWidget.MaxWidth, float.MaxValue).Deconstruct(out num, out num2);
				float num3 = num;
				if (num3 > TooltipWidget.MaxWidth)
				{
					Debug.LogWarning(string.Format("Tooltips title '{0}' is too long ({1}) for tooltip widget", title, num3));
				}
				if (description != null)
				{
					this.descriptionText.gameObject.SetActive(true);
					this.descriptionText.text = description;
					UiUtils.GetPreferredSize(this.descriptionText, TooltipWidget.MaxWidth, float.MaxValue).Deconstruct(out num2, out num);
					float num4 = num;
					vector.y += num4;
				}
				else
				{
					this.descriptionText.gameObject.SetActive(false);
					this.descriptionText.text = string.Empty;
				}
			}
			else
			{
				this.titleText.text = title;
				float num;
				float num2;
				UiUtils.GetPreferredSize(this.titleText, TooltipWidget.MaxWidth, float.MaxValue).Deconstruct(out num, out num2);
				float num5 = num;
				if (num5 > TooltipWidget.MaxWidth)
				{
					Debug.LogWarning(string.Format("Tooltips title '{0}' is too long ({1}) for tooltip widget", title, num5));
					vector.x = TooltipWidget.MaxWidth;
				}
				else
				{
					vector.x = num5;
				}
				if (description != null)
				{
					this.descriptionText.gameObject.SetActive(true);
					this.descriptionText.text = description;
					UiUtils.GetPreferredSize(this.descriptionText, TooltipWidget.MaxWidth, float.MaxValue).Deconstruct(out num2, out num);
					float num6 = num2;
					float num7 = num;
					float num8 = Mathf.Min(num6, TooltipWidget.MaxWidth);
					vector.x = Mathf.Max(num8, vector.x);
					vector.y += num7;
				}
				else
				{
					this.descriptionText.gameObject.SetActive(false);
					this.descriptionText.text = string.Empty;
				}
			}
			this.root.sizeDelta = vector;
		}

		// Token: 0x06000644 RID: 1604 RVA: 0x0001B068 File Offset: 0x00019268
		public void Set(TooltipSource source)
		{
			this.Source = source;
			if (source.Title == null)
			{
				Debug.LogError("Empty tooltip source on " + source.gameObject.name);
				return;
			}
			this.SetContent(false, source.Title, source.Description);
			float num = (GameMaster.IsLargeTooltips ? 1.5f : 1f);
			this.root.localScale = new Vector3(num, num, 1f);
		}

		// Token: 0x17000117 RID: 279
		// (get) Token: 0x06000645 RID: 1605 RVA: 0x0001B0DD File Offset: 0x000192DD
		public Vector2 Size
		{
			get
			{
				return this.root.sizeDelta * this.root.localScale;
			}
		}

		// Token: 0x040003D9 RID: 985
		[SerializeField]
		private RectTransform root;

		// Token: 0x040003DA RID: 986
		[SerializeField]
		private CanvasGroup canvasGroup;

		// Token: 0x040003DB RID: 987
		[SerializeField]
		private TextMeshProUGUI titleText;

		// Token: 0x040003DC RID: 988
		[SerializeField]
		private TextMeshProUGUI descriptionText;

		// Token: 0x040003DE RID: 990
		private const float FadeInDuration = 0.1f;

		// Token: 0x040003DF RID: 991
		private const float FadeInDelay = 0.1f;

		// Token: 0x040003E0 RID: 992
		private const float MaxWidthNormal = 500f;

		// Token: 0x040003E1 RID: 993
		private const float MaxWidthWide = 720f;
	}
}
