using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Presentation.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200003D RID: 61
	public class BattleManaWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
	{
		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x060003F0 RID: 1008 RVA: 0x0001022C File Offset: 0x0000E42C
		// (set) Token: 0x060003F1 RID: 1009 RVA: 0x0001024B File Offset: 0x0000E44B
		public BattleManaPanel Parent
		{
			get
			{
				BattleManaPanel battleManaPanel;
				if (!this._parent.TryGetTarget(ref battleManaPanel))
				{
					return null;
				}
				return battleManaPanel;
			}
			set
			{
				this._parent.SetTarget(value);
			}
		}

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x060003F2 RID: 1010 RVA: 0x00010259 File Offset: 0x0000E459
		private float TransitionDuration
		{
			get
			{
				if (!this.Parent)
				{
					return 0.1f;
				}
				return this.Parent.TransitionDuration;
			}
		}

		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x060003F3 RID: 1011 RVA: 0x00010279 File Offset: 0x0000E479
		// (set) Token: 0x060003F4 RID: 1012 RVA: 0x00010281 File Offset: 0x0000E481
		public ManaColor ManaColor { get; private set; }

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x060003F5 RID: 1013 RVA: 0x0001028A File Offset: 0x0000E48A
		// (set) Token: 0x060003F6 RID: 1014 RVA: 0x00010292 File Offset: 0x0000E492
		public Vector2 TargetPosition { get; private set; }

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x060003F7 RID: 1015 RVA: 0x0001029B File Offset: 0x0000E49B
		public Vector2 CurrentPosition
		{
			get
			{
				return this._rect.anchoredPosition;
			}
		}

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x060003F8 RID: 1016 RVA: 0x000102A8 File Offset: 0x0000E4A8
		// (set) Token: 0x060003F9 RID: 1017 RVA: 0x000102B0 File Offset: 0x0000E4B0
		public BattleManaStatus Status { get; private set; }

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x060003FA RID: 1018 RVA: 0x000102B9 File Offset: 0x0000E4B9
		// (set) Token: 0x060003FB RID: 1019 RVA: 0x000102C1 File Offset: 0x0000E4C1
		public bool IsPooled { get; private set; }

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x060003FC RID: 1020 RVA: 0x000102CA File Offset: 0x0000E4CA
		// (set) Token: 0x060003FD RID: 1021 RVA: 0x000102D4 File Offset: 0x0000E4D4
		public int Amount
		{
			get
			{
				return this._amount;
			}
			set
			{
				this._amount = value;
				if (value > 1)
				{
					this.amountText.text = value.ToString();
					this.amountText.gameObject.SetActive(true);
					return;
				}
				this.amountText.gameObject.SetActive(false);
			}
		}

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x060003FE RID: 1022 RVA: 0x00010321 File Offset: 0x0000E521
		// (set) Token: 0x060003FF RID: 1023 RVA: 0x00010329 File Offset: 0x0000E529
		public int Index { get; set; }

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x06000400 RID: 1024 RVA: 0x00010332 File Offset: 0x0000E532
		// (set) Token: 0x06000401 RID: 1025 RVA: 0x0001033A File Offset: 0x0000E53A
		private float ScaleValue
		{
			get
			{
				return this._scaleValue;
			}
			set
			{
				this._scaleValue = value;
				this.unpooledImage.color = Color.white.WithA(1f - value);
				this.pooledImage.color = Color.white.WithA(value);
			}
		}

		// Token: 0x170000AD RID: 173
		// (get) Token: 0x06000402 RID: 1026 RVA: 0x00010375 File Offset: 0x0000E575
		private bool IsInteractable
		{
			get
			{
				return this.Status == BattleManaStatus.Active;
			}
		}

		// Token: 0x06000403 RID: 1027 RVA: 0x00010380 File Offset: 0x0000E580
		private void Awake()
		{
			this._rect = (RectTransform)base.transform;
			this.amountText.gameObject.SetActive(false);
		}

		// Token: 0x06000404 RID: 1028 RVA: 0x000103A4 File Offset: 0x0000E5A4
		private void OnDestroy()
		{
			this.StopAllTweens();
		}

		// Token: 0x06000405 RID: 1029 RVA: 0x000103AC File Offset: 0x0000E5AC
		private void StopAllTweens()
		{
			this._rect.DOKill(false);
			this.canvasGroup.DOKill(false);
		}

		// Token: 0x06000406 RID: 1030 RVA: 0x000103C8 File Offset: 0x0000E5C8
		private void OnButtonClicked(bool rightClick)
		{
			BattleManaPanel parent = this.Parent;
			if (parent != null && parent)
			{
				parent.OnBattleManaClicked(this, rightClick);
			}
		}

		// Token: 0x06000407 RID: 1031 RVA: 0x000103EF File Offset: 0x0000E5EF
		public void Init(ManaColor color, Sprite normalSprite, Sprite highlightedSprite, Sprite normalPooledSprite, Sprite highlightedPooledSprite)
		{
			this.ManaColor = color;
			this.unpooledImage.sprite = normalSprite;
			this._unpooledImageHighlightSprite = highlightedSprite;
			this.pooledImage.sprite = normalPooledSprite;
			this._pooledImageHighlightSprite = highlightedPooledSprite;
		}

		// Token: 0x06000408 RID: 1032 RVA: 0x00010420 File Offset: 0x0000E620
		public void ResetStatus()
		{
			this.StopAllTweens();
			this.amountText.gameObject.SetActive(false);
			this.canvasGroup.interactable = false;
			this.Status = BattleManaStatus.Inactive;
		}

		// Token: 0x06000409 RID: 1033 RVA: 0x0001044C File Offset: 0x0000E64C
		public void Reinit(bool pooled)
		{
			this.IsPooled = pooled;
			this.ScaleValue = (float)(pooled ? 1 : 0);
		}

		// Token: 0x0600040A RID: 1034 RVA: 0x00010464 File Offset: 0x0000E664
		public void MoveTo(Vector2 toPosition, bool pooled, BattleManaStatus targetStatus)
		{
			BattleManaStatus status = this.Status;
			if (status == BattleManaStatus.Disappearing || status == BattleManaStatus.Inactive)
			{
				throw new InvalidOperationException(string.Format("Cannot move mana {0} while status = {1}", this.Index, this.Status));
			}
			this.Status = targetStatus;
			this.canvasGroup.interactable = targetStatus == BattleManaStatus.Active;
			if (this.IsPooled != pooled)
			{
				this.IsPooled = pooled;
				DOTween.To(() => this.ScaleValue, delegate(float x)
				{
					this.ScaleValue = x;
				}, (float)(pooled ? 1 : 0), this.TransitionDuration).SetUpdate(true);
			}
			this._rect.DOKill(false);
			this._rect.DOAnchorPos(toPosition, this.TransitionDuration, false).SetUpdate(true);
			this.TargetPosition = toPosition;
		}

		// Token: 0x0600040B RID: 1035 RVA: 0x0001052C File Offset: 0x0000E72C
		public void Appear(BattleManaStatus status, Vector2 fromPosition, Vector2? toPosition = null)
		{
			if (this.Status != BattleManaStatus.Inactive)
			{
				throw new InvalidOperationException("Cannot appear active mana widget");
			}
			this.Status = status;
			this.canvasGroup.alpha = 0f;
			this.canvasGroup.interactable = false;
			this._rect.anchoredPosition = fromPosition;
			if (toPosition != null)
			{
				Vector2 valueOrDefault = toPosition.GetValueOrDefault();
				this._rect.DOAnchorPos(valueOrDefault, this.TransitionDuration, false);
				this.TargetPosition = valueOrDefault;
			}
			else
			{
				this.TargetPosition = fromPosition;
			}
			this.canvasGroup.DOFade(1f, this.TransitionDuration).SetUpdate(true).OnComplete(delegate
			{
				this.canvasGroup.interactable = this.IsInteractable;
			});
		}

		// Token: 0x0600040C RID: 1036 RVA: 0x000105E0 File Offset: 0x0000E7E0
		public void InstantAppear(BattleManaStatus status, Vector2 position)
		{
			if (this.Status != BattleManaStatus.Inactive)
			{
				throw new InvalidOperationException("Cannot appear active mana widget");
			}
			this.Status = status;
			this.canvasGroup.alpha = 1f;
			this.canvasGroup.interactable = this.IsInteractable;
			this._rect.anchoredPosition = position;
			this.TargetPosition = position;
		}

		// Token: 0x0600040D RID: 1037 RVA: 0x0001063C File Offset: 0x0000E83C
		public void Disappear(Action onComplete = null)
		{
			this.canvasGroup.interactable = false;
			this._rect.DOKill(false);
			this.canvasGroup.DOKill(false);
			this.canvasGroup.DOFade(0f, this.TransitionDuration).SetUpdate(true).OnComplete(delegate
			{
				Action onComplete2 = onComplete;
				if (onComplete2 == null)
				{
					return;
				}
				onComplete2.Invoke();
			});
		}

		// Token: 0x0600040E RID: 1038 RVA: 0x000106AA File Offset: 0x0000E8AA
		public void Disappear(Vector2 toPosition, Action onComplete = null)
		{
			this.Disappear(onComplete);
			this._rect.DOAnchorPos(toPosition, this.TransitionDuration, false).SetUpdate(true);
		}

		// Token: 0x0600040F RID: 1039 RVA: 0x000106D0 File Offset: 0x0000E8D0
		public void Disappear(Vector2 toPosition, bool pooled, Action onComplete = null)
		{
			this.Disappear(onComplete);
			if (this.IsPooled != pooled)
			{
				this.IsPooled = pooled;
				DOTween.To(() => this.ScaleValue, delegate(float x)
				{
					this.ScaleValue = x;
				}, (float)(pooled ? 1 : 0), this.TransitionDuration).SetUpdate(true);
			}
			this._rect.DOAnchorPos(toPosition, this.TransitionDuration, false).SetUpdate(true);
		}

		// Token: 0x06000410 RID: 1040 RVA: 0x00010740 File Offset: 0x0000E940
		public void Consume(Action onComplete = null)
		{
			if (this.Status != BattleManaStatus.PendingUse)
			{
				throw new InvalidOperationException(string.Format("Cannot consume mana {0} while status = {1}", this.Index, this.Status));
			}
			this.Status = BattleManaStatus.Disappearing;
			this.Disappear(this.TargetPosition, onComplete);
		}

		// Token: 0x06000411 RID: 1041 RVA: 0x00010790 File Offset: 0x0000E990
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.unpooledImage.overrideSprite = this._unpooledImageHighlightSprite;
			this.pooledImage.overrideSprite = this._pooledImageHighlightSprite;
			UiManager.HoveringRightClickInteractionElements = true;
		}

		// Token: 0x06000412 RID: 1042 RVA: 0x000107BA File Offset: 0x0000E9BA
		public void OnPointerExit(PointerEventData eventData)
		{
			this.unpooledImage.overrideSprite = null;
			this.pooledImage.overrideSprite = null;
			UiManager.HoveringRightClickInteractionElements = false;
		}

		// Token: 0x06000413 RID: 1043 RVA: 0x000107DC File Offset: 0x0000E9DC
		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				AudioManager.PlayUi(this.IsPooled ? "ManaInactive" : "ManaActive", false);
				this.OnButtonClicked(false);
				return;
			}
			if (eventData.button == PointerEventData.InputButton.Right)
			{
				AudioManager.PlayUi(this.IsPooled ? "ManaInactive" : "ManaActive", false);
				this.OnButtonClicked(true);
			}
		}

		// Token: 0x040001D5 RID: 469
		[SerializeField]
		private CanvasGroup canvasGroup;

		// Token: 0x040001D6 RID: 470
		[SerializeField]
		private Image unpooledImage;

		// Token: 0x040001D7 RID: 471
		[SerializeField]
		private Image pooledImage;

		// Token: 0x040001D8 RID: 472
		[SerializeField]
		private TextMeshProUGUI amountText;

		// Token: 0x040001D9 RID: 473
		private Sprite _unpooledImageHighlightSprite;

		// Token: 0x040001DA RID: 474
		private Sprite _pooledImageHighlightSprite;

		// Token: 0x040001DB RID: 475
		private RectTransform _rect;

		// Token: 0x040001DC RID: 476
		private readonly WeakReference<BattleManaPanel> _parent = new WeakReference<BattleManaPanel>(null);

		// Token: 0x040001E1 RID: 481
		private int _amount = 1;

		// Token: 0x040001E3 RID: 483
		private float _scaleValue;
	}
}
