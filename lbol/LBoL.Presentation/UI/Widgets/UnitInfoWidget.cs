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
	public sealed class UnitInfoWidget : MonoBehaviour
	{
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
		private void OnEnable()
		{
			L10nManager.LocaleChanged += new Action(this.OnLocaleChanged);
			if (this._unit != null)
			{
				this.unitNameText.text = this._unit.Name;
			}
		}
		private void OnDisable()
		{
			L10nManager.LocaleChanged -= new Action(this.OnLocaleChanged);
		}
		private void OnLocaleChanged()
		{
			if (this._unit != null)
			{
				this.unitNameText.text = this._unit.Name;
			}
		}
		private void OnDestroy()
		{
			this.canvasGroup.DOKill(false);
			if (this._unit != null)
			{
				this.RemoveHandlers(this._unit);
			}
		}
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
		private void AddHandlers(Unit unit)
		{
			EnemyUnit enemyUnit = unit as EnemyUnit;
			if (enemyUnit != null)
			{
				enemyUnit.IntentionsChanged += new Action<EnemyUnit>(this.OnIntentionChanged);
			}
		}
		private void RemoveHandlers(Unit unit)
		{
			EnemyUnit enemyUnit = unit as EnemyUnit;
			if (enemyUnit != null)
			{
				enemyUnit.IntentionsChanged -= new Action<EnemyUnit>(this.OnIntentionChanged);
			}
		}
		private void OnIntentionChanged(EnemyUnit enemy)
		{
			this.SetIntentions(enemy);
		}
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
		public void ClearIntentions()
		{
			this.intentionLayout.DestroyChildren();
			this._intentionWidgets.Clear();
		}
		public void UpdateIntentions()
		{
			foreach (IntentionWidget intentionWidget in this._intentionWidgets)
			{
				intentionWidget.UpdateProperties();
			}
			this.UpdatePosition();
		}
		public void SetMoveOrder(int order)
		{
			this.moveOrderText.text = order.ToString();
		}
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
		public void ShowSePopup(string content, StatusEffectType seType, int number = 0, UnitInfoWidget.SePopType popType = UnitInfoWidget.SePopType.Add)
		{
			this._queue.Enqueue(new UnitInfoWidget.PopData(content, seType, number, popType));
			if (this._runner == null)
			{
				this._runner = base.StartCoroutine(this.ShowSePopupRunner());
			}
		}
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
		[SerializeField]
		private CanvasGroup canvasGroup;
		[SerializeField]
		private TextMeshProUGUI unitNameText;
		[SerializeField]
		private TextMeshProUGUI moveOrderText;
		[SerializeField]
		private RectTransform intentionRoot;
		[SerializeField]
		private IntentionWidget intentionTemplate;
		[SerializeField]
		private Transform intentionLayout;
		[SerializeField]
		private TextMeshProUGUI moveNameText;
		[SerializeField]
		private TextMeshProUGUI movePopupText;
		[SerializeField]
		private Transform sePopupRoot;
		[SerializeField]
		private TextMeshProUGUI sePopupTemplate;
		private Unit _unit;
		private ScenePositionTier _scenePositionTier;
		private readonly List<IntentionWidget> _intentionWidgets = new List<IntentionWidget>();
		private Vector3 _moveDefault;
		private Sequence _popupTween;
		private static readonly Color Green = new Color32(124, 241, 88, byte.MaxValue);
		private static readonly Color Red = new Color32(231, 58, 75, byte.MaxValue);
		private static readonly Color Blue = new Color32(116, 193, byte.MaxValue, byte.MaxValue);
		private const float Interval = 0.3f;
		private float _timer;
		private readonly Queue<UnitInfoWidget.PopData> _queue = new Queue<UnitInfoWidget.PopData>();
		private Coroutine _runner;
		private int _hoveringLevel;
		public enum SePopType
		{
			Add,
			Remove,
			Amulet
		}
		private class PopData
		{
			public PopData(string content, StatusEffectType seType, int number, UnitInfoWidget.SePopType popType)
			{
				this.Content = content;
				this.SeType = seType;
				this.Number = number;
				this.PopType = popType;
			}
			public string Content { get; }
			public StatusEffectType SeType { get; }
			public int Number { get; }
			public UnitInfoWidget.SePopType PopType { get; }
		}
	}
}
