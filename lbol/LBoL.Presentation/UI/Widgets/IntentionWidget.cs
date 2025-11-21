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
	// Token: 0x0200005C RID: 92
	public class IntentionWidget : MonoBehaviour
	{
		// Token: 0x170000DC RID: 220
		// (get) Token: 0x06000523 RID: 1315 RVA: 0x00016005 File Offset: 0x00014205
		private RectTransform RectTransform
		{
			get
			{
				return (RectTransform)base.transform;
			}
		}

		// Token: 0x170000DD RID: 221
		// (get) Token: 0x06000524 RID: 1316 RVA: 0x00016012 File Offset: 0x00014212
		public Intention Intention
		{
			get
			{
				return this._intention;
			}
		}

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x06000525 RID: 1317 RVA: 0x0001601A File Offset: 0x0001421A
		// (set) Token: 0x06000526 RID: 1318 RVA: 0x00016022 File Offset: 0x00014222
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

		// Token: 0x06000527 RID: 1319 RVA: 0x0001604A File Offset: 0x0001424A
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

		// Token: 0x06000528 RID: 1320 RVA: 0x00016084 File Offset: 0x00014284
		public float GetWidth()
		{
			if (this.text.gameObject.activeSelf)
			{
				return this.RectTransform.sizeDelta.x + this.text.GetPreferredValues(0f, 0f).x + this.text.rectTransform.anchoredPosition.x;
			}
			return this.RectTransform.sizeDelta.x;
		}

		// Token: 0x06000529 RID: 1321 RVA: 0x000160F5 File Offset: 0x000142F5
		private void Awake()
		{
			this._tooltip = base.gameObject.AddComponent<IntentionTooltipSource>();
			this._tooltip.widget = this;
			this.IsLargeTextSize = GameMaster.IsLargeTooltips;
		}

		// Token: 0x0600052A RID: 1322 RVA: 0x0001611F File Offset: 0x0001431F
		private void Start()
		{
			this.canvasGroup.DOFade(1f, 0.2f).From(0f, true, false).SetLink(base.gameObject);
		}

		// Token: 0x0600052B RID: 1323 RVA: 0x0001614E File Offset: 0x0001434E
		private void OnEnable()
		{
			L10nManager.LocaleChanged += new Action(this.OnLocaleChanged);
			this._changed = true;
		}

		// Token: 0x0600052C RID: 1324 RVA: 0x00016168 File Offset: 0x00014368
		private void OnDisable()
		{
			L10nManager.LocaleChanged -= new Action(this.OnLocaleChanged);
		}

		// Token: 0x0600052D RID: 1325 RVA: 0x0001617B File Offset: 0x0001437B
		private void OnLocaleChanged()
		{
			if (this._intention != null)
			{
				this._changed = true;
			}
		}

		// Token: 0x0600052E RID: 1326 RVA: 0x0001618C File Offset: 0x0001438C
		private void LateUpdate()
		{
			if (this._changed)
			{
				this.UpdateProperties();
				this._changed = false;
			}
		}

		// Token: 0x0600052F RID: 1327 RVA: 0x000161A3 File Offset: 0x000143A3
		private void OnDestroy()
		{
			this.ClearHandlers();
		}

		// Token: 0x06000530 RID: 1328 RVA: 0x000161AB File Offset: 0x000143AB
		private void AddHandlers()
		{
			if (this._intention != null)
			{
				this._intention.Activating += new Action(this.OnActivating);
			}
		}

		// Token: 0x06000531 RID: 1329 RVA: 0x000161CC File Offset: 0x000143CC
		private void ClearHandlers()
		{
			if (this._intention != null)
			{
				this._intention.Activating -= new Action(this.OnActivating);
			}
		}

		// Token: 0x06000532 RID: 1330 RVA: 0x000161ED File Offset: 0x000143ED
		private void OnActivating()
		{
			this.canvasGroup.DOKill(false);
			this.canvasGroup.DOFade(0f, 0.2f).SetLink(base.gameObject);
		}

		// Token: 0x06000533 RID: 1331 RVA: 0x00016220 File Offset: 0x00014420
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

		// Token: 0x040002EB RID: 747
		private Intention _intention;

		// Token: 0x040002EC RID: 748
		private IntentionTooltipSource _tooltip;

		// Token: 0x040002ED RID: 749
		private bool _changed;

		// Token: 0x040002EE RID: 750
		private bool _isLargeTextSize;

		// Token: 0x040002EF RID: 751
		private const float SmallTextSize = 48f;

		// Token: 0x040002F0 RID: 752
		private const float LargeTextSize = 60f;

		// Token: 0x040002F1 RID: 753
		public static readonly Color DamageColor = new Color32(221, 116, 115, byte.MaxValue);

		// Token: 0x040002F2 RID: 754
		public static readonly Color CountDownColor = new Color32(138, 181, 200, byte.MaxValue);

		// Token: 0x040002F3 RID: 755
		[SerializeField]
		private CanvasGroup canvasGroup;

		// Token: 0x040002F4 RID: 756
		[SerializeField]
		private Image intentionImage;

		// Token: 0x040002F5 RID: 757
		[SerializeField]
		private GameObject accuracyParticle;

		// Token: 0x040002F6 RID: 758
		[SerializeField]
		private TextMeshProUGUI text;
	}
}
