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
	// Token: 0x02000055 RID: 85
	public class ExhibitWidget : MonoBehaviour, IPointerExitHandler, IEventSystemHandler
	{
		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x060004DC RID: 1244 RVA: 0x00014E0C File Offset: 0x0001300C
		public Image MainImage
		{
			get
			{
				return this.image;
			}
		}

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x060004DD RID: 1245 RVA: 0x00014E14 File Offset: 0x00013014
		public RectTransform RectTransform
		{
			get
			{
				return (RectTransform)base.transform;
			}
		}

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x060004DE RID: 1246 RVA: 0x00014E21 File Offset: 0x00013021
		// (set) Token: 0x060004DF RID: 1247 RVA: 0x00014E29 File Offset: 0x00013029
		public bool ShowBattleStatus { get; set; }

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x060004E0 RID: 1248 RVA: 0x00014E32 File Offset: 0x00013032
		private static Color BlackoutColor
		{
			get
			{
				return new Color(0.5f, 0.5f, 0.5f);
			}
		}

		// Token: 0x060004E1 RID: 1249 RVA: 0x00014E48 File Offset: 0x00013048
		private void Awake()
		{
			this._canvasGroup = base.GetComponent<CanvasGroup>();
			this.commonButton.button.onClick.AddListener(new UnityAction(this.OnExhibitClicked));
		}

		// Token: 0x060004E2 RID: 1250 RVA: 0x00014E78 File Offset: 0x00013078
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

		// Token: 0x060004E3 RID: 1251 RVA: 0x00014F6C File Offset: 0x0001316C
		private void PlayActive()
		{
			this.activeParticle.gameObject.SetActive(true);
			this.activeParticle.Play();
			this._isActive = true;
		}

		// Token: 0x060004E4 RID: 1252 RVA: 0x00014F91 File Offset: 0x00013191
		private void CleanActive()
		{
			this.activeParticle.Stop();
			this.activeParticle.gameObject.SetActive(true);
			this._isActive = false;
		}

		// Token: 0x060004E5 RID: 1253 RVA: 0x00014FB6 File Offset: 0x000131B6
		private void PlayBlackout()
		{
			this._blackoutTween = this.image.DOColor(ExhibitWidget.BlackoutColor, 0.8f).SetEase(Ease.InCubic);
			this._isBlackout = true;
		}

		// Token: 0x060004E6 RID: 1254 RVA: 0x00014FE0 File Offset: 0x000131E0
		private void CleanBlackout()
		{
			this._blackoutTween.Kill(false);
			this.image.color = Color.white;
			this._blackoutTween = null;
			this._isBlackout = false;
		}

		// Token: 0x060004E7 RID: 1255 RVA: 0x0001500C File Offset: 0x0001320C
		private void OnEnable()
		{
			L10nManager.LocaleChanged += new Action(this.OnLocaleChanged);
		}

		// Token: 0x060004E8 RID: 1256 RVA: 0x0001501F File Offset: 0x0001321F
		private void OnDisable()
		{
			L10nManager.LocaleChanged -= new Action(this.OnLocaleChanged);
		}

		// Token: 0x060004E9 RID: 1257 RVA: 0x00015032 File Offset: 0x00013232
		private void OnLocaleChanged()
		{
			if (this._exhibit != null)
			{
				this._changed = true;
			}
		}

		// Token: 0x060004EA RID: 1258 RVA: 0x00015044 File Offset: 0x00013244
		private void OnDestroy()
		{
			if (this._exhibit != null)
			{
				this._exhibit.PropertyChanged -= new Action(this.OnPropertyChanged);
				this._exhibit.Activating -= new Action(this.OnActivating);
			}
			DOTween.Kill(this._canvasGroup, false);
		}

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x060004EB RID: 1259 RVA: 0x00015094 File Offset: 0x00013294
		public Vector3 CenterWorldPosition
		{
			get
			{
				RectTransform rectTransform = (RectTransform)base.transform;
				return rectTransform.TransformPoint(rectTransform.rect.center);
			}
		}

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x060004EC RID: 1260 RVA: 0x000150C4 File Offset: 0x000132C4
		// (set) Token: 0x060004ED RID: 1261 RVA: 0x000150D6 File Offset: 0x000132D6
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

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x060004EE RID: 1262 RVA: 0x000150E9 File Offset: 0x000132E9
		// (set) Token: 0x060004EF RID: 1263 RVA: 0x000150F4 File Offset: 0x000132F4
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

		// Token: 0x060004F0 RID: 1264 RVA: 0x000151F0 File Offset: 0x000133F0
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

		// Token: 0x060004F1 RID: 1265 RVA: 0x00015264 File Offset: 0x00013464
		private void OnPropertyChanged()
		{
			this._changed = true;
		}

		// Token: 0x060004F2 RID: 1266 RVA: 0x00015270 File Offset: 0x00013470
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

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x060004F3 RID: 1267 RVA: 0x000152C0 File Offset: 0x000134C0
		// (remove) Token: 0x060004F4 RID: 1268 RVA: 0x000152F8 File Offset: 0x000134F8
		public event Action ExhibitClicked;

		// Token: 0x060004F5 RID: 1269 RVA: 0x0001532D File Offset: 0x0001352D
		private void OnExhibitClicked()
		{
			Action exhibitClicked = this.ExhibitClicked;
			if (exhibitClicked == null)
			{
				return;
			}
			exhibitClicked.Invoke();
		}

		// Token: 0x14000006 RID: 6
		// (add) Token: 0x060004F6 RID: 1270 RVA: 0x00015340 File Offset: 0x00013540
		// (remove) Token: 0x060004F7 RID: 1271 RVA: 0x00015378 File Offset: 0x00013578
		public event Action<ExhibitWidget> ExhibitExit;

		// Token: 0x060004F8 RID: 1272 RVA: 0x000153AD File Offset: 0x000135AD
		public void OnPointerExit(PointerEventData eventData)
		{
			Action<ExhibitWidget> exhibitExit = this.ExhibitExit;
			if (exhibitExit == null)
			{
				return;
			}
			exhibitExit.Invoke(this);
		}

		// Token: 0x040002AA RID: 682
		[SerializeField]
		private Image image;

		// Token: 0x040002AB RID: 683
		[SerializeField]
		private Sprite defaultImage;

		// Token: 0x040002AC RID: 684
		[SerializeField]
		private TextMeshProUGUI counter;

		// Token: 0x040002AD RID: 685
		[SerializeField]
		private RectTransform effectRoot;

		// Token: 0x040002AE RID: 686
		[SerializeField]
		private AssociationList<Rarity, GameObject> effectTemplates;

		// Token: 0x040002AF RID: 687
		[SerializeField]
		private ExhibitActivating activatingParticle;

		// Token: 0x040002B0 RID: 688
		[SerializeField]
		private CommonButtonWidget commonButton;

		// Token: 0x040002B1 RID: 689
		[SerializeField]
		private ParticleSystem activeParticle;

		// Token: 0x040002B2 RID: 690
		private CanvasGroup _canvasGroup;

		// Token: 0x040002B3 RID: 691
		private bool _changed;

		// Token: 0x040002B4 RID: 692
		private bool _isActive;

		// Token: 0x040002B5 RID: 693
		private bool _isBlackout;

		// Token: 0x040002B7 RID: 695
		private Tween _blackoutTween;

		// Token: 0x040002B8 RID: 696
		private Exhibit _exhibit;
	}
}
