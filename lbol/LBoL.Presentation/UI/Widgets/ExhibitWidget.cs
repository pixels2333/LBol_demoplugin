using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Core;
using LBoL.Presentation.Effect;
using LBoL.Presentation.I10N;
using LBoL.Presentation.Units;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class ExhibitWidget : MonoBehaviour, IPointerExitHandler, IEventSystemHandler
	{
		public Image MainImage
		{
			get
			{
				return this.image;
			}
		}
		public RectTransform RectTransform
		{
			get
			{
				return (RectTransform)base.transform;
			}
		}
		public bool ShowBattleStatus { get; set; }
		private static Color BlackoutColor
		{
			get
			{
				return new Color(0.5f, 0.5f, 0.5f);
			}
		}
		private void Awake()
		{
			this._canvasGroup = base.GetComponent<CanvasGroup>();
			this.commonButton.button.onClick.AddListener(new UnityAction(this.OnExhibitClicked));
		}
		private void LateUpdate()
		{
			if (this._changed)
			{
				this._changed = false;
				this.counter.text = ((this._exhibit.HasCounter && this._exhibit.ShowCounter) ? this._exhibit.Counter.ToString() : string.Empty);
				string overrideIconName = this._exhibit.OverrideIconName;
				if (overrideIconName != null)
				{
					Sprite sprite = ResourcesHelper.TryGetSprite<Exhibit>(overrideIconName);
					if (sprite != null)
					{
						this.image.sprite = sprite;
					}
				}
			}
			if (this.ShowBattleStatus)
			{
				if (!this._isActive && this._exhibit.Active)
				{
					this.PlayActive();
				}
				if (this._isActive && !this._exhibit.Active)
				{
					this.CleanActive();
				}
				if (!this._isBlackout && this._exhibit.Blackout)
				{
					this.PlayBlackout();
				}
				if (this._isBlackout && !this._exhibit.Blackout)
				{
					this.CleanBlackout();
				}
			}
		}
		private void PlayActive()
		{
			this.activeParticle.gameObject.SetActive(true);
			this.activeParticle.Play();
			this._isActive = true;
		}
		private void CleanActive()
		{
			this.activeParticle.Stop();
			this.activeParticle.gameObject.SetActive(true);
			this._isActive = false;
		}
		private void PlayBlackout()
		{
			this._blackoutTween = this.image.DOColor(ExhibitWidget.BlackoutColor, 0.8f).SetEase(Ease.InCubic);
			this._isBlackout = true;
		}
		private void CleanBlackout()
		{
			this._blackoutTween.Kill(false);
			this.image.color = Color.white;
			this._blackoutTween = null;
			this._isBlackout = false;
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
			if (this._exhibit != null)
			{
				this._changed = true;
			}
		}
		private void OnDestroy()
		{
			if (this._exhibit != null)
			{
				this._exhibit.PropertyChanged -= new Action(this.OnPropertyChanged);
				this._exhibit.Activating -= new Action(this.OnActivating);
			}
			DOTween.Kill(this._canvasGroup, false);
		}
		public Vector3 CenterWorldPosition
		{
			get
			{
				RectTransform rectTransform = (RectTransform)base.transform;
				return rectTransform.TransformPoint(rectTransform.rect.center);
			}
		}
		public bool ShowCounter
		{
			get
			{
				return this.counter.gameObject.activeSelf;
			}
			set
			{
				this.counter.gameObject.SetActive(value);
			}
		}
		public Exhibit Exhibit
		{
			get
			{
				return this._exhibit;
			}
			set
			{
				if (this._exhibit != null)
				{
					this._exhibit.PropertyChanged -= new Action(this.OnPropertyChanged);
					this._exhibit.Activating -= new Action(this.OnActivating);
				}
				this._exhibit = value;
				if (value != null)
				{
					value.PropertyChanged += new Action(this.OnPropertyChanged);
					value.Activating += new Action(this.OnActivating);
					Sprite sprite = ResourcesHelper.TryGetSprite<Exhibit>(value.OverrideIconName ?? value.IconName);
					if (sprite != null)
					{
						this.image.sprite = sprite;
					}
					else
					{
						this.image.sprite = this.defaultImage;
					}
					this.counter.text = ((value.HasCounter && value.ShowCounter) ? value.Counter.ToString() : string.Empty);
					return;
				}
				this.image.sprite = null;
				this.counter.text = string.Empty;
			}
		}
		private void OnActivating()
		{
			DOTween.Kill(this._canvasGroup, true);
			this._canvasGroup.DOFade(0f, 0.1f).SetLoops(6, LoopType.Yoyo);
			if (this._exhibit.Battle != null)
			{
				ExhibitActivating exhibitActivating = Object.Instantiate<ExhibitActivating>(this.activatingParticle, GameDirector.Player.EffectRoot);
				exhibitActivating.OnPropertyChanged(this._exhibit);
				Object.Destroy(exhibitActivating.gameObject, 2f);
			}
		}
		private void OnPropertyChanged()
		{
			this._changed = true;
		}
		public void ShowObtained(bool instant = false)
		{
			GameObject gameObject;
			if (this.effectTemplates.TryGetValue(this._exhibit.Config.Rarity, out gameObject))
			{
				GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject, this.effectRoot);
				gameObject2.GetComponentInChildren<ParticleSystem>().Play();
				Object.Destroy(gameObject2, 2f);
			}
		}
		public event Action ExhibitClicked;
		private void OnExhibitClicked()
		{
			Action exhibitClicked = this.ExhibitClicked;
			if (exhibitClicked == null)
			{
				return;
			}
			exhibitClicked.Invoke();
		}
		public event Action<ExhibitWidget> ExhibitExit;
		public void OnPointerExit(PointerEventData eventData)
		{
			Action<ExhibitWidget> exhibitExit = this.ExhibitExit;
			if (exhibitExit == null)
			{
				return;
			}
			exhibitExit.Invoke(this);
		}
		[SerializeField]
		private Image image;
		[SerializeField]
		private Sprite defaultImage;
		[SerializeField]
		private TextMeshProUGUI counter;
		[SerializeField]
		private RectTransform effectRoot;
		[SerializeField]
		private AssociationList<Rarity, GameObject> effectTemplates;
		[SerializeField]
		private ExhibitActivating activatingParticle;
		[SerializeField]
		private CommonButtonWidget commonButton;
		[SerializeField]
		private ParticleSystem activeParticle;
		private CanvasGroup _canvasGroup;
		private bool _changed;
		private bool _isActive;
		private bool _isBlackout;
		private Tween _blackoutTween;
		private Exhibit _exhibit;
	}
}
