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
	public class BattleManaWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
	{
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
		public ManaColor ManaColor { get; private set; }
		public Vector2 TargetPosition { get; private set; }
		public Vector2 CurrentPosition
		{
			get
			{
				return this._rect.anchoredPosition;
			}
		}
		public BattleManaStatus Status { get; private set; }
		public bool IsPooled { get; private set; }
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
		public int Index { get; set; }
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
		private bool IsInteractable
		{
			get
			{
				return this.Status == BattleManaStatus.Active;
			}
		}
		private void Awake()
		{
			this._rect = (RectTransform)base.transform;
			this.amountText.gameObject.SetActive(false);
		}
		private void OnDestroy()
		{
			this.StopAllTweens();
		}
		private void StopAllTweens()
		{
			this._rect.DOKill(false);
			this.canvasGroup.DOKill(false);
		}
		private void OnButtonClicked(bool rightClick)
		{
			BattleManaPanel parent = this.Parent;
			if (parent != null && parent)
			{
				parent.OnBattleManaClicked(this, rightClick);
			}
		}
		public void Init(ManaColor color, Sprite normalSprite, Sprite highlightedSprite, Sprite normalPooledSprite, Sprite highlightedPooledSprite)
		{
			this.ManaColor = color;
			this.unpooledImage.sprite = normalSprite;
			this._unpooledImageHighlightSprite = highlightedSprite;
			this.pooledImage.sprite = normalPooledSprite;
			this._pooledImageHighlightSprite = highlightedPooledSprite;
		}
		public void ResetStatus()
		{
			this.StopAllTweens();
			this.amountText.gameObject.SetActive(false);
			this.canvasGroup.interactable = false;
			this.Status = BattleManaStatus.Inactive;
		}
		public void Reinit(bool pooled)
		{
			this.IsPooled = pooled;
			this.ScaleValue = (float)(pooled ? 1 : 0);
		}
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
		public void Disappear(Vector2 toPosition, Action onComplete = null)
		{
			this.Disappear(onComplete);
			this._rect.DOAnchorPos(toPosition, this.TransitionDuration, false).SetUpdate(true);
		}
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
		public void Consume(Action onComplete = null)
		{
			if (this.Status != BattleManaStatus.PendingUse)
			{
				throw new InvalidOperationException(string.Format("Cannot consume mana {0} while status = {1}", this.Index, this.Status));
			}
			this.Status = BattleManaStatus.Disappearing;
			this.Disappear(this.TargetPosition, onComplete);
		}
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.unpooledImage.overrideSprite = this._unpooledImageHighlightSprite;
			this.pooledImage.overrideSprite = this._pooledImageHighlightSprite;
			UiManager.HoveringRightClickInteractionElements = true;
		}
		public void OnPointerExit(PointerEventData eventData)
		{
			this.unpooledImage.overrideSprite = null;
			this.pooledImage.overrideSprite = null;
			UiManager.HoveringRightClickInteractionElements = false;
		}
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
		[SerializeField]
		private CanvasGroup canvasGroup;
		[SerializeField]
		private Image unpooledImage;
		[SerializeField]
		private Image pooledImage;
		[SerializeField]
		private TextMeshProUGUI amountText;
		private Sprite _unpooledImageHighlightSprite;
		private Sprite _pooledImageHighlightSprite;
		private RectTransform _rect;
		private readonly WeakReference<BattleManaPanel> _parent = new WeakReference<BattleManaPanel>(null);
		private int _amount = 1;
		private float _scaleValue;
	}
}
