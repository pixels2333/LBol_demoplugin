using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base.Extensions;
using LBoL.Core.Intentions;
using LBoL.Core.Units;
using LBoL.Presentation.I10N;
using LBoL.Presentation.UI.ExtraWidgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class IntentionWidget : MonoBehaviour
	{
		private RectTransform RectTransform
		{
			get
			{
				return (RectTransform)base.transform;
			}
		}
		public Intention Intention
		{
			get
			{
				return this._intention;
			}
		}
		public bool IsLargeTextSize
		{
			get
			{
				return this._isLargeTextSize;
			}
			set
			{
				this._isLargeTextSize = value;
				this.text.fontSize = (this._isLargeTextSize ? 60f : 48f);
			}
		}
		public void SetIntention(Intention intention, bool forceUpdate)
		{
			this.ClearHandlers();
			this._intention = intention;
			if (intention != null)
			{
				this.canvasGroup.alpha = 1f;
				if (forceUpdate)
				{
					this.UpdateProperties();
				}
				else
				{
					this._changed = true;
				}
				this.AddHandlers();
			}
		}
		public float GetWidth()
		{
			if (this.text.gameObject.activeSelf)
			{
				return this.RectTransform.sizeDelta.x + this.text.GetPreferredValues(0f, 0f).x + this.text.rectTransform.anchoredPosition.x;
			}
			return this.RectTransform.sizeDelta.x;
		}
		private void Awake()
		{
			this._tooltip = base.gameObject.AddComponent<IntentionTooltipSource>();
			this._tooltip.widget = this;
			this.IsLargeTextSize = GameMaster.IsLargeTooltips;
		}
		private void Start()
		{
			this.canvasGroup.DOFade(1f, 0.2f).From(0f, true, false).SetLink(base.gameObject);
		}
		private void OnEnable()
		{
			L10nManager.LocaleChanged += new Action(this.OnLocaleChanged);
			this._changed = true;
		}
		private void OnDisable()
		{
			L10nManager.LocaleChanged -= new Action(this.OnLocaleChanged);
		}
		private void OnLocaleChanged()
		{
			if (this._intention != null)
			{
				this._changed = true;
			}
		}
		private void LateUpdate()
		{
			if (this._changed)
			{
				this.UpdateProperties();
				this._changed = false;
			}
		}
		private void OnDestroy()
		{
			this.ClearHandlers();
		}
		private void AddHandlers()
		{
			if (this._intention != null)
			{
				this._intention.Activating += new Action(this.OnActivating);
			}
		}
		private void ClearHandlers()
		{
			if (this._intention != null)
			{
				this._intention.Activating -= new Action(this.OnActivating);
			}
		}
		private void OnActivating()
		{
			this.canvasGroup.DOKill(false);
			this.canvasGroup.DOFade(0f, 0.2f).SetLink(base.gameObject);
		}
		public void UpdateProperties()
		{
			this.accuracyParticle.gameObject.SetActive(false);
			string text = null;
			string text2;
			if (this._intention.HiddenFinal)
			{
				text2 = "UnknownIntention";
				this.text.gameObject.SetActive(false);
			}
			else
			{
				Intention intention = this._intention;
				AttackIntention attackIntention = intention as AttackIntention;
				if (attackIntention == null)
				{
					ExplodeIntention explodeIntention = intention as ExplodeIntention;
					if (explodeIntention == null)
					{
						SpellCardIntention spellCardIntention = intention as SpellCardIntention;
						if (spellCardIntention == null)
						{
							CountDownIntention countDownIntention = intention as CountDownIntention;
							if (countDownIntention == null)
							{
								KokoroDarkIntention kokoroDarkIntention = intention as KokoroDarkIntention;
								if (kokoroDarkIntention == null)
								{
									this.text.gameObject.SetActive(false);
								}
								else
								{
									string damageText = kokoroDarkIntention.DamageText;
									if (damageText != null)
									{
										this.text.color = IntentionWidget.DamageColor;
										this.text.gameObject.SetActive(true);
										this.text.text = damageText;
									}
									else
									{
										this.text.gameObject.SetActive(false);
									}
								}
							}
							else
							{
								this.text.text = countDownIntention.Counter.ToString();
								this.text.color = IntentionWidget.CountDownColor;
								this.text.gameObject.SetActive(true);
							}
						}
						else
						{
							string damageText2 = spellCardIntention.DamageText;
							if (damageText2 != null)
							{
								this.text.color = IntentionWidget.DamageColor;
								this.text.gameObject.SetActive(true);
								this.text.text = damageText2;
							}
							else
							{
								this.text.gameObject.SetActive(false);
							}
							if (spellCardIntention.IsAccuracy)
							{
								this.text.ForceMeshUpdate(false, false);
								this.accuracyParticle.gameObject.SetActive(true);
							}
						}
					}
					else
					{
						this.text.text = explodeIntention.DamageText;
						this.text.color = IntentionWidget.DamageColor;
						this.text.gameObject.SetActive(true);
					}
				}
				else
				{
					this.text.text = attackIntention.DamageText;
					this.text.color = IntentionWidget.DamageColor;
					this.text.gameObject.SetActive(true);
					if (attackIntention.IsAccuracy)
					{
						this.text.ForceMeshUpdate(false, false);
						this.accuracyParticle.gameObject.SetActive(true);
					}
					int totalDamage = attackIntention.TotalDamage;
					string text3;
					if (totalDamage >= 10)
					{
						if (totalDamage >= 30)
						{
							text3 = "3";
						}
						else
						{
							text3 = "2";
						}
					}
					else
					{
						text3 = "1";
					}
					text = text3;
				}
				text2 = this._intention.GetType().Name;
				if (!this._intention.SpecialIconName.IsNullOrEmpty())
				{
					text2 = this._intention.SpecialIconName;
				}
			}
			Sprite sprite = ResourcesHelper.TryGetIntention(text2, text);
			if (sprite != null)
			{
				this.intentionImage.sprite = sprite;
				return;
			}
			Debug.LogWarning("Lack of Intention Sprite: " + this._intention.GetType().Name);
		}
		private Intention _intention;
		private IntentionTooltipSource _tooltip;
		private bool _changed;
		private bool _isLargeTextSize;
		private const float SmallTextSize = 48f;
		private const float LargeTextSize = 60f;
		public static readonly Color DamageColor = new Color32(221, 116, 115, byte.MaxValue);
		public static readonly Color CountDownColor = new Color32(138, 181, 200, byte.MaxValue);
		[SerializeField]
		private CanvasGroup canvasGroup;
		[SerializeField]
		private Image intentionImage;
		[SerializeField]
		private GameObject accuracyParticle;
		[SerializeField]
		private TextMeshProUGUI text;
	}
}
