using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x0200009C RID: 156
	public sealed class HintPanel : UiPanel<HintPayload>, IInputActionHandler
	{
		// Token: 0x1700015B RID: 347
		// (get) Token: 0x0600081F RID: 2079 RVA: 0x00026410 File Offset: 0x00024610
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}

		// Token: 0x1700015C RID: 348
		// (get) Token: 0x06000820 RID: 2080 RVA: 0x00026413 File Offset: 0x00024613
		private RectTransform RectTransform
		{
			get
			{
				return (RectTransform)base.transform;
			}
		}

		// Token: 0x06000821 RID: 2081 RVA: 0x00026420 File Offset: 0x00024620
		private void Awake()
		{
			this.hintLeftBg.color = Color.black.WithA(0.95f);
			this.hintRightBg.color = Color.black.WithA(0.95f);
			this.hintTopBg.color = Color.black.WithA(0.95f);
			this.hintBottomBg.color = Color.black.WithA(0.95f);
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}

		// Token: 0x06000822 RID: 2082 RVA: 0x000264A4 File Offset: 0x000246A4
		public override async UniTask CustomLocalizationAsync()
		{
			try
			{
				Dictionary<string, HintPanel.Entry> dictionary = await Localization.LoadFileAsync<HintPanel.Entry>("Hint");
				this._l10nTable = dictionary;
			}
			catch (Exception ex)
			{
				Debug.LogError("[HintPanel] localization failed: " + ex.Message);
			}
		}

		// Token: 0x1700015D RID: 349
		// (get) Token: 0x06000823 RID: 2083 RVA: 0x000264E7 File Offset: 0x000246E7
		// (set) Token: 0x06000824 RID: 2084 RVA: 0x000264F0 File Offset: 0x000246F0
		public Rect HintBgRect
		{
			get
			{
				return this._hintBgRect;
			}
			set
			{
				this._hintBgRect = value;
				float num;
				float num2;
				float num3;
				float num4;
				this.RectTransform.rect.Deconstruct(out num, out num2, out num3, out num4);
				float num5 = num;
				float num6 = num2;
				float num7 = num3;
				float num8 = num4;
				num4 = num5 + num7;
				float num9 = num6 + num8;
				float num10 = num4;
				float num11 = num9;
				value.Deconstruct(out num4, out num3, out num2, out num);
				float num12 = num4;
				float num13 = num3;
				float num14 = num2;
				float num15 = num;
				num = num12 + num14;
				float num16 = num13 + num15;
				float num17 = num;
				float num18 = num16;
				this.hintLeftBg.rectTransform.localPosition = new Vector3((num5 + num12) / 2f, 0f);
				this.hintLeftBg.rectTransform.sizeDelta = new Vector3(num12 - num5, num8);
				this.hintRightBg.rectTransform.localPosition = new Vector3((num10 + num17) / 2f, 0f);
				this.hintRightBg.rectTransform.sizeDelta = new Vector2(num10 - num17, num8);
				float x = value.center.x;
				this.hintBottomBg.rectTransform.localPosition = new Vector3(x, (num6 + num13) / 2f);
				this.hintBottomBg.rectTransform.sizeDelta = new Vector2(num14, num13 - num6);
				this.hintTopBg.rectTransform.localPosition = new Vector3(x, (num18 + num11) / 2f);
				this.hintTopBg.rectTransform.sizeDelta = new Vector2(num14, num11 - num18);
			}
		}

		// Token: 0x06000825 RID: 2085 RVA: 0x00026674 File Offset: 0x00024874
		protected override void OnShowing(HintPayload payload)
		{
			if (this._copyedGameObject)
			{
				Object.Destroy(this._copyedGameObject.gameObject);
			}
			string hintKey = payload.HintKey;
			GameMaster.MarkHintAsShown(hintKey);
			HintPanel.Entry entry;
			if (!this._l10nTable.TryGetValue(hintKey, ref entry))
			{
				Debug.LogError("[HintPanel] Cannot get hint localization for '" + hintKey + "'");
				entry = new HintPanel.Entry
				{
					Title = "<" + hintKey + ".Title>",
					Content = "<" + hintKey + ".Description"
				};
			}
			this.hintTitle.text = entry.Title;
			this.hintContent.text = entry.Content;
			DOTween.Kill(this, false);
			if (payload.Target)
			{
				this.hintBackground.gameObject.SetActive(false);
				this.hintLeftBg.gameObject.SetActive(true);
				this.hintRightBg.gameObject.SetActive(true);
				this.hintTopBg.gameObject.SetActive(true);
				this.hintBottomBg.gameObject.SetActive(true);
				if (payload.CopyedGameObject)
				{
					this._copyedGameObject = payload.CopyedGameObject;
					this._copyedGameObject.SetParent(this.RectTransform);
				}
				TooltipPositioner orAddComponent = this.hintRootTrans.gameObject.GetOrAddComponent<TooltipPositioner>();
				orAddComponent.TooltipSize = this.hintRootTrans.sizeDelta;
				orAddComponent.TooltipPositions = HintPanel.DefaultPositions;
				orAddComponent.Gap = 20f;
				orAddComponent.ForceUpdateTo(payload.Target);
				Object.Destroy(orAddComponent);
				Vector3[] array = new Vector3[4];
				payload.Target.GetWorldCorners(array);
				for (int i = 0; i < 4; i++)
				{
					array[i] = this.RectTransform.InverseTransformPoint(array[i]);
				}
				float num = Enumerable.Min<Vector3>(array, (Vector3 v) => v.x);
				float num2 = Enumerable.Max<Vector3>(array, (Vector3 v) => v.x);
				float num3 = Enumerable.Min<Vector3>(array, (Vector3 v) => v.y);
				float num4 = Enumerable.Max<Vector3>(array, (Vector3 v) => v.y);
				Rect rect = Rect.MinMaxRect(num, num3, num2, num4);
				this.HintBgRect = this.RectTransform.rect;
				DOTween.To(() => this.HintBgRect, delegate(Rect x)
				{
					this.HintBgRect = x;
				}, rect, 0.3f).SetDelay(payload.Delay).SetUpdate(true)
					.SetTarget(this)
					.SetLink(base.gameObject);
			}
			else
			{
				this.hintBackground.gameObject.SetActive(true);
				this.hintLeftBg.gameObject.SetActive(false);
				this.hintRightBg.gameObject.SetActive(false);
				this.hintTopBg.gameObject.SetActive(false);
				this.hintBottomBg.gameObject.SetActive(false);
				this.hintRootTrans.localPosition = Vector3.zero;
				DOTween.Kill(this, false);
				this.hintBackground.color = Color.black.WithA(0f);
				this.hintBackground.DOFade(0.95f, 0.3f).SetDelay(payload.Delay).SetUpdate(true)
					.SetTarget(this)
					.SetLink(base.gameObject);
			}
			CanvasGroup orAddComponent2 = this.hintRootTrans.gameObject.GetOrAddComponent<CanvasGroup>();
			orAddComponent2.alpha = 0f;
			orAddComponent2.DOFade(1f, 0.3f).SetDelay(payload.Delay).SetUpdate(true)
				.SetTarget(this)
				.SetLink(base.gameObject);
			this._canvasGroup.interactable = true;
			UiManager.PushActionHandler(this);
		}

		// Token: 0x06000826 RID: 2086 RVA: 0x00026A61 File Offset: 0x00024C61
		protected override void OnHiding()
		{
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
		}

		// Token: 0x06000827 RID: 2087 RVA: 0x00026A75 File Offset: 0x00024C75
		protected override void OnHided()
		{
			if (this._copyedGameObject)
			{
				Object.Destroy(this._copyedGameObject.gameObject);
			}
		}

		// Token: 0x06000828 RID: 2088 RVA: 0x00026A94 File Offset: 0x00024C94
		void IInputActionHandler.OnConfirm()
		{
			base.Hide();
		}

		// Token: 0x06000829 RID: 2089 RVA: 0x00026A9C File Offset: 0x00024C9C
		void IInputActionHandler.OnCancel()
		{
			base.Hide();
		}

		// Token: 0x0600082A RID: 2090 RVA: 0x00026AA4 File Offset: 0x00024CA4
		public IEnumerator ShowAsync(HintPayload payload)
		{
			base.Show(payload);
			yield return new WaitWhile(() => base.IsVisible);
			yield break;
		}

		// Token: 0x040005A3 RID: 1443
		private static readonly TooltipPosition[] DefaultPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max)
		};

		// Token: 0x040005A4 RID: 1444
		[SerializeField]
		private RectTransform hintRootTrans;

		// Token: 0x040005A5 RID: 1445
		[SerializeField]
		private TextMeshProUGUI hintTitle;

		// Token: 0x040005A6 RID: 1446
		[SerializeField]
		private TextMeshProUGUI hintContent;

		// Token: 0x040005A7 RID: 1447
		[SerializeField]
		private RawImage hintBackground;

		// Token: 0x040005A8 RID: 1448
		[SerializeField]
		private RawImage hintLeftBg;

		// Token: 0x040005A9 RID: 1449
		[SerializeField]
		private RawImage hintRightBg;

		// Token: 0x040005AA RID: 1450
		[SerializeField]
		private RawImage hintTopBg;

		// Token: 0x040005AB RID: 1451
		[SerializeField]
		private RawImage hintBottomBg;

		// Token: 0x040005AC RID: 1452
		private IDictionary<string, HintPanel.Entry> _l10nTable;

		// Token: 0x040005AD RID: 1453
		private RectTransform _copyedGameObject;

		// Token: 0x040005AE RID: 1454
		private CanvasGroup _canvasGroup;

		// Token: 0x040005AF RID: 1455
		private const float FadeInDuration = 0.3f;

		// Token: 0x040005B0 RID: 1456
		private const float BgFinalAlpha = 0.95f;

		// Token: 0x040005B1 RID: 1457
		private Rect _hintBgRect;

		// Token: 0x02000262 RID: 610
		private sealed class Entry
		{
			// Token: 0x1700043D RID: 1085
			// (get) Token: 0x06001558 RID: 5464 RVA: 0x0006246F File Offset: 0x0006066F
			// (set) Token: 0x06001559 RID: 5465 RVA: 0x00062477 File Offset: 0x00060677
			public string Title
			{
				get; [UsedImplicitly]
				set;
			}

			// Token: 0x1700043E RID: 1086
			// (get) Token: 0x0600155A RID: 5466 RVA: 0x00062480 File Offset: 0x00060680
			// (set) Token: 0x0600155B RID: 5467 RVA: 0x00062488 File Offset: 0x00060688
			public string Content
			{
				get; [UsedImplicitly]
				set;
			}
		}
	}
}
