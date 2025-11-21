using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.Presentation.I10N;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200007B RID: 123
	public sealed class UnitInfoWidget : MonoBehaviour
	{
		// Token: 0x17000118 RID: 280
		// (get) Token: 0x06000647 RID: 1607 RVA: 0x0001B107 File Offset: 0x00019307
		// (set) Token: 0x06000648 RID: 1608 RVA: 0x0001B114 File Offset: 0x00019314
		public float Alpha
		{
			get
			{
				return this.canvasGroup.alpha;
			}
			set
			{
				this.canvasGroup.alpha = value;
			}
		}

		// Token: 0x06000649 RID: 1609 RVA: 0x0001B122 File Offset: 0x00019322
		public void SetVisible(bool visible, bool instant = false)
		{
			this.canvasGroup.DOKill(false);
			if (instant)
			{
				this.canvasGroup.alpha = (float)(visible ? 1 : 0);
				return;
			}
			this.canvasGroup.DOFade((float)(visible ? 1 : 0), 0.2f);
		}

		// Token: 0x0600064A RID: 1610 RVA: 0x0001B164 File Offset: 0x00019364
		private void Awake()
		{
			this.unitNameText.gameObject.SetActive(false);
			this.intentionTemplate.gameObject.SetActive(false);
			this._scenePositionTier = base.GetComponent<ScenePositionTier>();
			if (this._scenePositionTier == null)
			{
				this._scenePositionTier = base.gameObject.AddComponent<ScenePositionTier>();
			}
			this._moveDefault = this.movePopupText.transform.localPosition;
			this.moveOrderText.gameObject.SetActive(false);
			this.moveNameText.gameObject.SetActive(false);
			this.movePopupText.gameObject.SetActive(false);
			this.sePopupTemplate.gameObject.SetActive(false);
			this.intentionRoot.localPosition = Vector3.zero;
		}

		// Token: 0x0600064B RID: 1611 RVA: 0x0001B228 File Offset: 0x00019428
		private void OnEnable()
		{
			L10nManager.LocaleChanged += new Action(this.OnLocaleChanged);
			if (this._unit != null)
			{
				this.unitNameText.text = this._unit.Name;
			}
		}

		// Token: 0x0600064C RID: 1612 RVA: 0x0001B259 File Offset: 0x00019459
		private void OnDisable()
		{
			L10nManager.LocaleChanged -= new Action(this.OnLocaleChanged);
		}

		// Token: 0x0600064D RID: 1613 RVA: 0x0001B26C File Offset: 0x0001946C
		private void OnLocaleChanged()
		{
			if (this._unit != null)
			{
				this.unitNameText.text = this._unit.Name;
			}
		}

		// Token: 0x0600064E RID: 1614 RVA: 0x0001B28C File Offset: 0x0001948C
		private void OnDestroy()
		{
			this.canvasGroup.DOKill(false);
			if (this._unit != null)
			{
				this.RemoveHandlers(this._unit);
			}
		}

		// Token: 0x17000119 RID: 281
		// (get) Token: 0x0600064F RID: 1615 RVA: 0x0001B2AF File Offset: 0x000194AF
		// (set) Token: 0x06000650 RID: 1616 RVA: 0x0001B2B7 File Offset: 0x000194B7
		public Unit Unit
		{
			get
			{
				return this._unit;
			}
			set
			{
				if (this._unit != null)
				{
					this.RemoveHandlers(this._unit);
				}
				this._unit = value;
				if (value != null)
				{
					this.unitNameText.text = this._unit.Name;
					this.AddHandlers(value);
				}
			}
		}

		// Token: 0x06000651 RID: 1617 RVA: 0x0001B2F4 File Offset: 0x000194F4
		private void AddHandlers(Unit unit)
		{
			EnemyUnit enemyUnit = unit as EnemyUnit;
			if (enemyUnit != null)
			{
				enemyUnit.IntentionsChanged += new Action<EnemyUnit>(this.OnIntentionChanged);
			}
		}

		// Token: 0x06000652 RID: 1618 RVA: 0x0001B320 File Offset: 0x00019520
		private void RemoveHandlers(Unit unit)
		{
			EnemyUnit enemyUnit = unit as EnemyUnit;
			if (enemyUnit != null)
			{
				enemyUnit.IntentionsChanged -= new Action<EnemyUnit>(this.OnIntentionChanged);
			}
		}

		// Token: 0x06000653 RID: 1619 RVA: 0x0001B349 File Offset: 0x00019549
		private void OnIntentionChanged(EnemyUnit enemy)
		{
			this.SetIntentions(enemy);
		}

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x06000654 RID: 1620 RVA: 0x0001B352 File Offset: 0x00019552
		// (set) Token: 0x06000655 RID: 1621 RVA: 0x0001B35F File Offset: 0x0001955F
		public Transform TargetTransform
		{
			get
			{
				return this._scenePositionTier.TargetTransform;
			}
			set
			{
				this._scenePositionTier.TargetTransform = value;
			}
		}

		// Token: 0x06000656 RID: 1622 RVA: 0x0001B370 File Offset: 0x00019570
		private void SetIntentions(EnemyUnit enemy)
		{
			this.ClearIntentions();
			List<Intention> intentions = enemy.Intentions;
			if (intentions == null)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (Intention intention in intentions)
			{
				IntentionWidget intentionWidget = Object.Instantiate<IntentionWidget>(this.intentionTemplate, this.intentionLayout);
				intentionWidget.gameObject.SetActive(true);
				intentionWidget.SetIntention(intention, true);
				this._intentionWidgets.Add(intentionWidget);
				list.Add(intention.MoveName);
			}
			if (this._unit.Battle.HideEnemyIntention)
			{
				this.moveNameText.gameObject.SetActive(false);
			}
			else
			{
				string text = Enumerable.FirstOrDefault<string>(list, (string s) => !s.IsNullOrEmpty());
				this.moveNameText.text = text;
				this.moveNameText.gameObject.SetActive(!text.IsNullOrEmpty());
			}
			this.UpdatePosition();
		}

		// Token: 0x06000657 RID: 1623 RVA: 0x0001B48C File Offset: 0x0001968C
		private void UpdatePosition()
		{
			float num = -Enumerable.Sum<IntentionWidget>(this._intentionWidgets, (IntentionWidget intention) => intention.GetWidth()) / 2f;
			foreach (IntentionWidget intentionWidget in this._intentionWidgets)
			{
				RectTransform rectTransform = (RectTransform)intentionWidget.transform;
				rectTransform.anchoredPosition = new Vector2(num + rectTransform.sizeDelta.x / 2f, 0f);
				num += intentionWidget.GetWidth();
			}
		}

		// Token: 0x06000658 RID: 1624 RVA: 0x0001B544 File Offset: 0x00019744
		public void ClearIntentions()
		{
			this.intentionLayout.DestroyChildren();
			this._intentionWidgets.Clear();
		}

		// Token: 0x06000659 RID: 1625 RVA: 0x0001B55C File Offset: 0x0001975C
		public void UpdateIntentions()
		{
			foreach (IntentionWidget intentionWidget in this._intentionWidgets)
			{
				intentionWidget.UpdateProperties();
			}
			this.UpdatePosition();
		}

		// Token: 0x0600065A RID: 1626 RVA: 0x0001B5B4 File Offset: 0x000197B4
		public void SetMoveOrder(int order)
		{
			this.moveOrderText.text = order.ToString();
		}

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x0600065B RID: 1627 RVA: 0x0001B5C8 File Offset: 0x000197C8
		// (set) Token: 0x0600065C RID: 1628 RVA: 0x0001B5DA File Offset: 0x000197DA
		public bool IsMoveOrderVisible
		{
			get
			{
				return this.moveOrderText.gameObject.activeSelf;
			}
			set
			{
				this.moveOrderText.gameObject.SetActive(value);
			}
		}

		// Token: 0x0600065D RID: 1629 RVA: 0x0001B5F0 File Offset: 0x000197F0
		public void ShowMovePopup(string moveName, bool closeMoveName)
		{
			Transform transform = this.movePopupText.transform;
			if (this._popupTween.IsActive())
			{
				this._popupTween.Kill(true);
			}
			this.movePopupText.gameObject.SetActive(true);
			this.moveNameText.gameObject.SetActive(false);
			this.movePopupText.text = moveName;
			this.movePopupText.transform.localPosition = this._moveDefault;
			this.movePopupText.transform.localScale = Vector3.one;
			this.movePopupText.color = this.movePopupText.color.WithA(1f);
			this._popupTween = DOTween.Sequence().Join(transform.DOLocalMoveY(40f, 1f, false).SetRelative(true).SetEase(Ease.OutSine)).Insert(0.7f, transform.DOScale(1.2f, 0.3f).SetEase(Ease.InSine))
				.Insert(0.8f, this.movePopupText.DOFade(0f, 0.2f))
				.SetUpdate(true)
				.OnComplete(delegate
				{
					this.movePopupText.gameObject.SetActive(false);
					if (!closeMoveName)
					{
						this.moveNameText.gameObject.SetActive(true);
					}
				});
		}

		// Token: 0x0600065E RID: 1630 RVA: 0x0001B732 File Offset: 0x00019932
		public void ShowSePopup(string content, StatusEffectType seType, int number = 0, UnitInfoWidget.SePopType popType = UnitInfoWidget.SePopType.Add)
		{
			this._queue.Enqueue(new UnitInfoWidget.PopData(content, seType, number, popType));
			if (this._runner == null)
			{
				this._runner = base.StartCoroutine(this.ShowSePopupRunner());
			}
		}

		// Token: 0x0600065F RID: 1631 RVA: 0x0001B763 File Offset: 0x00019963
		private IEnumerator ShowSePopupRunner()
		{
			this._timer = 0f;
			while (this._queue.Count > 0)
			{
				this._timer += Time.deltaTime;
				if (this._timer > 0.3f)
				{
					this._timer = 0f;
					UnitInfoWidget.PopData popData = this._queue.Dequeue();
					this.InternalShowPopup(popData.Content, popData.SeType, popData.Number, popData.PopType);
				}
				yield return null;
			}
			this._runner = null;
			yield break;
		}

		// Token: 0x06000660 RID: 1632 RVA: 0x0001B774 File Offset: 0x00019974
		private void InternalShowPopup(string content, StatusEffectType seType, int number, UnitInfoWidget.SePopType popType)
		{
			TextMeshProUGUI textMeshProUGUI = Object.Instantiate<TextMeshProUGUI>(this.sePopupTemplate, this.sePopupRoot);
			GameObject obj = textMeshProUGUI.gameObject;
			if (number > 0)
			{
				content = "+" + number.ToString() + " " + content;
			}
			switch (popType)
			{
			case UnitInfoWidget.SePopType.Add:
			{
				TextMeshProUGUI textMeshProUGUI2 = textMeshProUGUI;
				Color color;
				switch (seType)
				{
				case StatusEffectType.Positive:
					color = UnitInfoWidget.Green;
					break;
				case StatusEffectType.Negative:
					color = UnitInfoWidget.Red;
					break;
				case StatusEffectType.Special:
					color = UnitInfoWidget.Blue;
					break;
				default:
					throw new ArgumentOutOfRangeException("seType", seType, null);
				}
				textMeshProUGUI2.color = color;
				break;
			}
			case UnitInfoWidget.SePopType.Remove:
				textMeshProUGUI.color = Color.white;
				content = "UI.SeRemove".LocalizeFormat(new object[] { content });
				break;
			case UnitInfoWidget.SePopType.Amulet:
				textMeshProUGUI.color = Color.white;
				content = "UI.SeAmulet".LocalizeFormat(new object[] { content });
				break;
			default:
				throw new ArgumentOutOfRangeException("popType", popType, null);
			}
			textMeshProUGUI.text = content;
			Transform transform = textMeshProUGUI.transform;
			obj.SetActive(true);
			DOTween.Sequence().Join(transform.DOLocalMoveY(120f, 1f, false).SetRelative(true).SetEase(Ease.OutSine)).Join(transform.DOScale(1.3f, 0.5f).SetDelay(0.5f).SetEase(Ease.InSine))
				.SetLink(obj)
				.SetUpdate(true)
				.OnComplete(delegate
				{
					Object.Destroy(obj);
				});
		}

		// Token: 0x06000661 RID: 1633 RVA: 0x0001B90C File Offset: 0x00019B0C
		public void IncreaseHoveringLevel()
		{
			this._hoveringLevel++;
			if (this._hoveringLevel > 0)
			{
				this.intentionRoot.DOKill(false);
				this.intentionRoot.DOLocalMoveY(this.unitNameText.rectTransform.sizeDelta.y, 0.1f, false).SetUpdate(true).OnComplete(delegate
				{
					this.unitNameText.gameObject.SetActive(true);
				});
			}
		}

		// Token: 0x06000662 RID: 1634 RVA: 0x0001B97C File Offset: 0x00019B7C
		public void DecreaseHoveringLevel()
		{
			this._hoveringLevel--;
			if (this._hoveringLevel <= 0)
			{
				this.unitNameText.gameObject.SetActive(false);
				this.intentionRoot.DOKill(false);
				this.intentionRoot.DOLocalMoveY(0f, 0.1f, false).SetUpdate(true);
			}
		}

		// Token: 0x040003E2 RID: 994
		[SerializeField]
		private CanvasGroup canvasGroup;

		// Token: 0x040003E3 RID: 995
		[SerializeField]
		private TextMeshProUGUI unitNameText;

		// Token: 0x040003E4 RID: 996
		[SerializeField]
		private TextMeshProUGUI moveOrderText;

		// Token: 0x040003E5 RID: 997
		[SerializeField]
		private RectTransform intentionRoot;

		// Token: 0x040003E6 RID: 998
		[SerializeField]
		private IntentionWidget intentionTemplate;

		// Token: 0x040003E7 RID: 999
		[SerializeField]
		private Transform intentionLayout;

		// Token: 0x040003E8 RID: 1000
		[SerializeField]
		private TextMeshProUGUI moveNameText;

		// Token: 0x040003E9 RID: 1001
		[SerializeField]
		private TextMeshProUGUI movePopupText;

		// Token: 0x040003EA RID: 1002
		[SerializeField]
		private Transform sePopupRoot;

		// Token: 0x040003EB RID: 1003
		[SerializeField]
		private TextMeshProUGUI sePopupTemplate;

		// Token: 0x040003EC RID: 1004
		private Unit _unit;

		// Token: 0x040003ED RID: 1005
		private ScenePositionTier _scenePositionTier;

		// Token: 0x040003EE RID: 1006
		private readonly List<IntentionWidget> _intentionWidgets = new List<IntentionWidget>();

		// Token: 0x040003EF RID: 1007
		private Vector3 _moveDefault;

		// Token: 0x040003F0 RID: 1008
		private Sequence _popupTween;

		// Token: 0x040003F1 RID: 1009
		private static readonly Color Green = new Color32(124, 241, 88, byte.MaxValue);

		// Token: 0x040003F2 RID: 1010
		private static readonly Color Red = new Color32(231, 58, 75, byte.MaxValue);

		// Token: 0x040003F3 RID: 1011
		private static readonly Color Blue = new Color32(116, 193, byte.MaxValue, byte.MaxValue);

		// Token: 0x040003F4 RID: 1012
		private const float Interval = 0.3f;

		// Token: 0x040003F5 RID: 1013
		private float _timer;

		// Token: 0x040003F6 RID: 1014
		private readonly Queue<UnitInfoWidget.PopData> _queue = new Queue<UnitInfoWidget.PopData>();

		// Token: 0x040003F7 RID: 1015
		private Coroutine _runner;

		// Token: 0x040003F8 RID: 1016
		private int _hoveringLevel;

		// Token: 0x020001E1 RID: 481
		public enum SePopType
		{
			// Token: 0x04000F34 RID: 3892
			Add,
			// Token: 0x04000F35 RID: 3893
			Remove,
			// Token: 0x04000F36 RID: 3894
			Amulet
		}

		// Token: 0x020001E2 RID: 482
		private class PopData
		{
			// Token: 0x0600135B RID: 4955 RVA: 0x0005A100 File Offset: 0x00058300
			public PopData(string content, StatusEffectType seType, int number, UnitInfoWidget.SePopType popType)
			{
				this.Content = content;
				this.SeType = seType;
				this.Number = number;
				this.PopType = popType;
			}

			// Token: 0x170003E1 RID: 993
			// (get) Token: 0x0600135C RID: 4956 RVA: 0x0005A125 File Offset: 0x00058325
			public string Content { get; }

			// Token: 0x170003E2 RID: 994
			// (get) Token: 0x0600135D RID: 4957 RVA: 0x0005A12D File Offset: 0x0005832D
			public StatusEffectType SeType { get; }

			// Token: 0x170003E3 RID: 995
			// (get) Token: 0x0600135E RID: 4958 RVA: 0x0005A135 File Offset: 0x00058335
			public int Number { get; }

			// Token: 0x170003E4 RID: 996
			// (get) Token: 0x0600135F RID: 4959 RVA: 0x0005A13D File Offset: 0x0005833D
			public UnitInfoWidget.SePopType PopType { get; }
		}
	}
}
