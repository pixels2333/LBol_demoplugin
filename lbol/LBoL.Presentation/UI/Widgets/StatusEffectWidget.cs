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
	// Token: 0x02000075 RID: 117
	public class StatusEffectWidget : MonoBehaviour
	{
		// Token: 0x060005F3 RID: 1523 RVA: 0x00019BC4 File Offset: 0x00017DC4
		private void Awake()
		{
			this.effectImage.gameObject.SetActive(false);
		}

		// Token: 0x060005F4 RID: 1524 RVA: 0x00019BD8 File Offset: 0x00017DD8
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

		// Token: 0x060005F5 RID: 1525 RVA: 0x00019C55 File Offset: 0x00017E55
		private void PlayHighlight()
		{
			this._isHighlight = true;
			this.hintParticle.gameObject.SetActive(true);
			this.hintParticle.Play();
		}

		// Token: 0x060005F6 RID: 1526 RVA: 0x00019C7A File Offset: 0x00017E7A
		private void CleanHighlight()
		{
			this._isHighlight = false;
			this.hintParticle.Stop();
			this.hintParticle.gameObject.SetActive(true);
		}

		// Token: 0x060005F7 RID: 1527 RVA: 0x00019C9F File Offset: 0x00017E9F
		private void OnEnable()
		{
			L10nManager.LocaleChanged += new Action(this.OnLocaleChanged);
		}

		// Token: 0x060005F8 RID: 1528 RVA: 0x00019CB2 File Offset: 0x00017EB2
		private void OnDisable()
		{
			L10nManager.LocaleChanged -= new Action(this.OnLocaleChanged);
		}

		// Token: 0x060005F9 RID: 1529 RVA: 0x00019CC5 File Offset: 0x00017EC5
		private void OnLocaleChanged()
		{
			if (this._statusEffect != null)
			{
				this._changed = true;
			}
		}

		// Token: 0x060005FA RID: 1530 RVA: 0x00019CD8 File Offset: 0x00017ED8
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

		// Token: 0x060005FB RID: 1531 RVA: 0x00019D2F File Offset: 0x00017F2F
		private void AddHandlers(StatusEffect effect)
		{
			effect.Activating += new Action(this.OnActivating);
			effect.PropertyChanged += new Action(this.OnPropertyChanged);
		}

		// Token: 0x060005FC RID: 1532 RVA: 0x00019D55 File Offset: 0x00017F55
		private void RemoveHandlers(StatusEffect effect)
		{
			effect.PropertyChanged -= new Action(this.OnPropertyChanged);
			effect.Activating -= new Action(this.OnActivating);
		}

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x060005FD RID: 1533 RVA: 0x00019D7C File Offset: 0x00017F7C
		public Vector3 CenterWorldPosition
		{
			get
			{
				RectTransform rectTransform = (RectTransform)base.transform;
				return rectTransform.TransformPoint(rectTransform.rect.center);
			}
		}

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x060005FE RID: 1534 RVA: 0x00019DAC File Offset: 0x00017FAC
		// (set) Token: 0x060005FF RID: 1535 RVA: 0x00019DB4 File Offset: 0x00017FB4
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

		// Token: 0x06000600 RID: 1536 RVA: 0x00019DE1 File Offset: 0x00017FE1
		private void OnActivating()
		{
			this.ShowActivating();
		}

		// Token: 0x06000601 RID: 1537 RVA: 0x00019DEC File Offset: 0x00017FEC
		private void OnPropertyChanged()
		{
			this._changed = true;
			string unitEffectName = this._statusEffect.UnitEffectName;
			if (unitEffectName != null)
			{
				GameDirector.GetUnit(this._statusEffect.Owner).SendEffectMessage(unitEffectName, "OnPropertyChanged", this._statusEffect);
			}
		}

		// Token: 0x06000602 RID: 1538 RVA: 0x00019E30 File Offset: 0x00018030
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

		// Token: 0x06000603 RID: 1539 RVA: 0x00019EA8 File Offset: 0x000180A8
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

		// Token: 0x06000604 RID: 1540 RVA: 0x00019FBC File Offset: 0x000181BC
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

		// Token: 0x06000605 RID: 1541 RVA: 0x0001A084 File Offset: 0x00018284
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

		// Token: 0x040003AE RID: 942
		[SerializeField]
		private Image image;

		// Token: 0x040003AF RID: 943
		[SerializeField]
		private Image effectImage;

		// Token: 0x040003B0 RID: 944
		[SerializeField]
		private TextMeshProUGUI upText;

		// Token: 0x040003B1 RID: 945
		[SerializeField]
		private TextMeshProUGUI downText;

		// Token: 0x040003B2 RID: 946
		[SerializeField]
		private ParticleSystem hintParticle;

		// Token: 0x040003B3 RID: 947
		private StatusEffect _statusEffect;

		// Token: 0x040003B4 RID: 948
		private bool _changed;

		// Token: 0x040003B5 RID: 949
		private bool _isHighlight;
	}
}
