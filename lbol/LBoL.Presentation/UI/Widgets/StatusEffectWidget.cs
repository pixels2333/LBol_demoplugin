using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Presentation.I10N;
using LBoL.Presentation.Units;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class StatusEffectWidget : MonoBehaviour
	{
		private void Awake()
		{
			this.effectImage.gameObject.SetActive(false);
		}
		private void LateUpdate()
		{
			if (this._changed)
			{
				this._changed = false;
				this.TextRefresh();
				string overrideIconName = this._statusEffect.OverrideIconName;
				if (overrideIconName != null)
				{
					Sprite sprite = ResourcesHelper.TryGetSprite<StatusEffect>(overrideIconName);
					if (sprite != null)
					{
						this.image.sprite = sprite;
					}
				}
			}
			if (!this._isHighlight && this._statusEffect.Highlight)
			{
				this.PlayHighlight();
			}
			if (this._isHighlight && !this._statusEffect.Highlight)
			{
				this.CleanHighlight();
			}
		}
		private void PlayHighlight()
		{
			this._isHighlight = true;
			this.hintParticle.gameObject.SetActive(true);
			this.hintParticle.Play();
		}
		private void CleanHighlight()
		{
			this._isHighlight = false;
			this.hintParticle.Stop();
			this.hintParticle.gameObject.SetActive(true);
		}
		private void OnEnable()
		{
			L10nManager.LocaleChanged += new Action(this.OnLocaleChanged);
		}
		private void OnDisable()
		{
			L10nManager.LocaleChanged -= new Action(this.OnLocaleChanged);
		}
		private void OnLocaleChanged()
		{
			if (this._statusEffect != null)
			{
				this._changed = true;
			}
		}
		private void OnDestroy()
		{
			if (this._statusEffect != null)
			{
				this._statusEffect.PropertyChanged -= new Action(this.OnPropertyChanged);
				this._statusEffect.Activating -= new Action(this.OnActivating);
			}
			this._statusEffect = null;
			this.effectImage.DOKill(false);
		}
		private void AddHandlers(StatusEffect effect)
		{
			effect.Activating += new Action(this.OnActivating);
			effect.PropertyChanged += new Action(this.OnPropertyChanged);
		}
		private void RemoveHandlers(StatusEffect effect)
		{
			effect.PropertyChanged -= new Action(this.OnPropertyChanged);
			effect.Activating -= new Action(this.OnActivating);
		}
		public Vector3 CenterWorldPosition
		{
			get
			{
				RectTransform rectTransform = (RectTransform)base.transform;
				return rectTransform.TransformPoint(rectTransform.rect.center);
			}
		}
		public StatusEffect StatusEffect
		{
			get
			{
				return this._statusEffect;
			}
			set
			{
				if (this._statusEffect != null)
				{
					this.RemoveHandlers(this._statusEffect);
				}
				this._statusEffect = value;
				if (value != null)
				{
					this.AddHandlers(value);
				}
				this.Refresh();
			}
		}
		private void OnActivating()
		{
			this.ShowActivating();
		}
		private void OnPropertyChanged()
		{
			this._changed = true;
			string unitEffectName = this._statusEffect.UnitEffectName;
			if (unitEffectName != null)
			{
				GameDirector.GetUnit(this._statusEffect.Owner).SendEffectMessage(unitEffectName, "OnPropertyChanged", this._statusEffect);
			}
		}
		public void Refresh()
		{
			if (this._statusEffect != null)
			{
				Sprite sprite = ResourcesHelper.TryGetSprite<StatusEffect>(this._statusEffect.OverrideIconName ?? this._statusEffect.IconName);
				if (sprite != null)
				{
					this.image.sprite = sprite;
				}
				this.TextRefresh();
				return;
			}
			this.image.sprite = null;
			this.upText.text = string.Empty;
			this.downText.text = string.Empty;
		}
		private void TextRefresh()
		{
			this.upText.text = ((this._statusEffect.HasCount && this._statusEffect.ShowCount) ? this._statusEffect.Count.ToString() : string.Empty);
			if (this._statusEffect.ForceNotShowDownText)
			{
				this.downText.text = string.Empty;
				return;
			}
			if (this._statusEffect.HasLevel)
			{
				this.downText.text = this._statusEffect.Level.ToString();
				this.downText.color = GlobalConfig.UiRed;
				if (this._statusEffect.HasDuration)
				{
					Debug.LogWarning(base.name + " has both Level and Duration, which cause UI display problem.");
					return;
				}
			}
			else
			{
				if (this._statusEffect.HasDuration)
				{
					this.downText.text = this._statusEffect.Duration.ToString();
					this.downText.color = GlobalConfig.UiBlue;
					return;
				}
				this.downText.text = string.Empty;
			}
		}
		public YieldInstruction ShowAdded()
		{
			this.effectImage.gameObject.SetActive(true);
			this.effectImage.sprite = this.image.sprite;
			this.effectImage.DOKill(false);
			return DOTween.Sequence().Append(this.effectImage.transform.DOScale(Vector3.one * 3f, 0.8f).From(Vector3.one, true, false)).Join(this.effectImage.DOFade(0f, 0.8f).From(1f, true, false))
				.SetEase(Ease.OutQuad)
				.OnComplete(delegate
				{
					this.effectImage.gameObject.SetActive(false);
				})
				.SetTarget(this.effectImage)
				.WaitForCompletion();
		}
		public YieldInstruction ShowActivating()
		{
			this.effectImage.gameObject.SetActive(true);
			this.effectImage.sprite = this.image.sprite;
			this.effectImage.DOKill(false);
			return DOTween.Sequence().Append(this.effectImage.transform.DOScale(Vector3.one, 0.8f).From(Vector3.one * 3f, true, false)).Join(this.effectImage.DOFade(1f, 0.4f).From(0f, true, false).SetLoops(1, LoopType.Yoyo))
				.SetEase(Ease.OutQuad)
				.OnComplete(delegate
				{
					this.effectImage.gameObject.SetActive(false);
				})
				.SetTarget(this.effectImage)
				.WaitForCompletion();
		}
		[SerializeField]
		private Image image;
		[SerializeField]
		private Image effectImage;
		[SerializeField]
		private TextMeshProUGUI upText;
		[SerializeField]
		private TextMeshProUGUI downText;
		[SerializeField]
		private ParticleSystem hintParticle;
		private StatusEffect _statusEffect;
		private bool _changed;
		private bool _isHighlight;
	}
}
