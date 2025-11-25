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
	public class TooltipWidget : MonoBehaviour
	{
		public TooltipSource Source { get; private set; }
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
		private void OnEnable()
		{
			this.canvasGroup.DOFade(1f, 0.1f).From(0f, true, false).SetDelay(0.1f)
				.SetUpdate(true);
		}
		private void OnDestroy()
		{
			this.canvasGroup.DOKill(false);
		}
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
		public Vector2 Size
		{
			get
			{
				return this.root.sizeDelta * this.root.localScale;
			}
		}
		[SerializeField]
		private RectTransform root;
		[SerializeField]
		private CanvasGroup canvasGroup;
		[SerializeField]
		private TextMeshProUGUI titleText;
		[SerializeField]
		private TextMeshProUGUI descriptionText;
		private const float FadeInDuration = 0.1f;
		private const float FadeInDelay = 0.1f;
		private const float MaxWidthNormal = 500f;
		private const float MaxWidthWide = 720f;
	}
}
