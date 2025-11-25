using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Stations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class MapNodeWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		public Vector2 Grid { get; set; }
		public MapNode MapNode { get; private set; }
		public MapNodeWidget.WidgetStatus Status { get; private set; }
		private bool ButtonActive { get; set; }
		public int X
		{
			get
			{
				return this.MapNode.X;
			}
		}
		public int Y
		{
			get
			{
				return this.MapNode.Y;
			}
		}
		public int StageLevel { get; set; }
		private void Awake()
		{
			this.button.onClick.AddListener(new UnityAction(this.OnButtonClicked));
		}
		public void Initialize(MapNode mapNode, MapNodeWidget.WidgetStatus status, RandomGen uiRng, int stageLevel)
		{
			this.MapNode = mapNode;
			this.Status = status;
			this.StageLevel = stageLevel;
			if (this.signalImage)
			{
				this.signalImage.fillAmount = (float)((this.Status == MapNodeWidget.WidgetStatus.Visited) ? 1 : 0);
			}
			if (this.outerImage)
			{
				this.outerImage.color = ((this.Status == MapNodeWidget.WidgetStatus.Visiting) ? Color.white : new Color(1f, 1f, 1f, 0f));
			}
			if (this.Status == MapNodeWidget.WidgetStatus.Passed)
			{
				this.baseImage.color = this._passedColor;
				this.iconImage.color = this._passedColor;
			}
			StationType stationType = mapNode.StationType;
			if (stationType <= StationType.EliteEnemy)
			{
				if (stationType == StationType.Enemy)
				{
					this.iconImage.gameObject.SetActive(true);
					List<Sprite> list;
					switch (mapNode.Act)
					{
					case 1:
						list = this.normalEnemySprites1;
						break;
					case 2:
						list = this.normalEnemySprites2;
						break;
					case 3:
						list = this.normalEnemySprites3;
						break;
					default:
						throw new ArgumentOutOfRangeException(string.Format("Invalid map node act: {0}", mapNode.Act));
					}
					List<Sprite> list2 = list;
					this.iconImage.sprite = list2.Sample(uiRng);
					return;
				}
				if (stationType == StationType.EliteEnemy)
				{
					this.iconImage.gameObject.SetActive(true);
					this.iconImage.sprite = this.eliteEnemySprites.Sample(uiRng);
					return;
				}
			}
			else
			{
				if (stationType == StationType.Trade)
				{
					this.iconImage.gameObject.SetActive(true);
					this.iconImage.sprite = ((stageLevel == 3) ? this.tradeSprites[1] : this.tradeSprites[0]);
					return;
				}
				if (stationType == StationType.Boss)
				{
					this.iconImage.gameObject.SetActive(true);
					this.ConvertToBossWidget();
					string bossId = mapNode.Map.BossId;
					if (bossId == null)
					{
						return;
					}
					Sprite sprite = ResourcesHelper.TryGetBossIcon(bossId);
					if (sprite)
					{
						this.iconImage.sprite = sprite;
						return;
					}
					this.bossName.text = bossId;
					this.iconImage.gameObject.SetActive(false);
					this.bossName.gameObject.SetActive(true);
					return;
				}
			}
			this.SetIconDefault();
		}
		private void SetIconDefault()
		{
			if (this.iconTable.ContainsKey(this.MapNode.StationType))
			{
				this.iconImage.sprite = this.iconTable[this.MapNode.StationType];
				this.iconImage.gameObject.SetActive(true);
				return;
			}
			this.iconImage.gameObject.SetActive(false);
		}
		private void ConvertToBossWidget()
		{
			base.GetComponent<RectTransform>().sizeDelta = new Vector2(800f, 800f);
			this.baseImage.sprite = this.bossBase;
			this.iconImage.sprite = this.bossUnselect;
			this.outerImage.sprite = this.bossOuter;
		}
		private void OnButtonClicked()
		{
			if (!this.ButtonActive)
			{
				Debug.Log("Mapnode button is inactive.");
				return;
			}
			AudioManager.Button(0);
			Action click = this.Click;
			if (click == null)
			{
				return;
			}
			click.Invoke();
		}
		public event Action Click;
		public void SetStatus(MapNodeWidget.WidgetStatus status)
		{
			this.Status = status;
			this.outerImage.color = ((this.Status == MapNodeWidget.WidgetStatus.Visiting) ? Color.white : new Color(1f, 1f, 1f, 0f));
			if (this.Status == MapNodeWidget.WidgetStatus.Visited && this.signalImage.fillAmount < 1f)
			{
				this.signalImage.fillAmount = 1f;
			}
			if (this.Status == MapNodeWidget.WidgetStatus.Passed)
			{
				this.baseImage.color = this._passedColor;
				this.iconImage.color = this._passedColor;
			}
			this.StartRootScaleTween();
			if (this.Status == MapNodeWidget.WidgetStatus.Active || this.Status == MapNodeWidget.WidgetStatus.CrossActive)
			{
				this.ButtonActive = true;
				return;
			}
			this.EndRootScaleTween();
			this.ButtonActive = false;
		}
		public void SetStatus(MapNode node)
		{
			switch (node.Status)
			{
			case MapNodeStatus.NotVisited:
				this.SetStatus(MapNodeWidget.WidgetStatus.NotVisited);
				return;
			case MapNodeStatus.Active:
				this.SetStatus(MapNodeWidget.WidgetStatus.Active);
				return;
			case MapNodeStatus.CrossActive:
				this.SetStatus(MapNodeWidget.WidgetStatus.CrossActive);
				return;
			case MapNodeStatus.Visiting:
				this.SetStatus(MapNodeWidget.WidgetStatus.Visiting);
				return;
			case MapNodeStatus.Visited:
				this.SetStatus(MapNodeWidget.WidgetStatus.Visited);
				return;
			case MapNodeStatus.Passed:
				this.SetStatus(MapNodeWidget.WidgetStatus.Passed);
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		public void SetButtonInactive()
		{
			this.ButtonActive = false;
		}
		public void PlayFinish()
		{
			if (this.signalImage.fillAmount < 1f)
			{
				this.signalImage.DOFillAmount(1f, 0.5f).SetAutoKill<TweenerCore<float, float, FloatOptions>>().SetUpdate(true);
			}
		}
		public void OnPointerEnter(PointerEventData eventData)
		{
			if (this.Status == MapNodeWidget.WidgetStatus.Active)
			{
				AudioManager.Button(2);
				this.outerImage.DOKill(false);
				this.outerImage.DOFade(1f, 0.1f).SetAutoKill<TweenerCore<Color, Color, ColorOptions>>().SetUpdate(true);
			}
			this.root.DOKill(false);
			this.root.DOScale(1.1f, 0.1f).SetAutoKill<TweenerCore<Vector3, Vector3, VectorOptions>>().SetUpdate(true);
		}
		public void OnPointerExit(PointerEventData eventData)
		{
			if (this.Status == MapNodeWidget.WidgetStatus.Active)
			{
				this.outerImage.DOKill(false);
				this.outerImage.DOFade(0f, 0.1f).SetAutoKill<TweenerCore<Color, Color, ColorOptions>>().SetUpdate(true);
			}
			this.root.DOKill(false);
			this.root.DOScale(1f, 0.1f).SetAutoKill<TweenerCore<Vector3, Vector3, VectorOptions>>().OnComplete(new TweenCallback(this.StartRootScaleTween))
				.SetUpdate(true);
		}
		public void StartRootScaleTween()
		{
			if (this.root && (this.Status == MapNodeWidget.WidgetStatus.Active || this.Status == MapNodeWidget.WidgetStatus.CrossActive))
			{
				this.root.DOScale(1.1f, 0.5f).From(1f, true, false).SetLoops(-1, LoopType.Yoyo)
					.SetAutoKill<TweenerCore<Vector3, Vector3, VectorOptions>>()
					.SetUpdate(true);
			}
			if (this.Status == MapNodeWidget.WidgetStatus.CrossActive)
			{
				this.crossingOuterImage.DOFade(1f, 0.1f).SetUpdate(true);
				return;
			}
			this.crossingOuterImage.DOFade(0f, 0.1f).SetUpdate(true);
		}
		public void EndRootScaleTween()
		{
			this.root.DOKill(false);
			this.root.localScale = Vector3.one;
		}
		public void SetBoss(string bossId)
		{
			this.iconImage.sprite = ResourcesHelper.TryGetBossIcon(bossId);
		}
		[SerializeField]
		private AssociationList<StationType, Sprite> iconTable;
		[SerializeField]
		private List<Sprite> tradeSprites;
		[SerializeField]
		private List<Sprite> normalEnemySprites1;
		[SerializeField]
		private List<Sprite> normalEnemySprites2;
		[SerializeField]
		private List<Sprite> normalEnemySprites3;
		[SerializeField]
		private List<Sprite> eliteEnemySprites;
		[SerializeField]
		private Sprite defaultBase;
		[SerializeField]
		private Sprite colorBase;
		[SerializeField]
		private Sprite bossBase;
		[SerializeField]
		private Sprite bossOuter;
		[SerializeField]
		private Sprite bossUnselect;
		[SerializeField]
		private TextMeshProUGUI bossName;
		[SerializeField]
		private Button button;
		[SerializeField]
		private Transform root;
		[SerializeField]
		private Image baseImage;
		[SerializeField]
		private Image outerImage;
		[SerializeField]
		private Image crossingOuterImage;
		[SerializeField]
		private Image iconImage;
		[SerializeField]
		private Image signalImage;
		private readonly Color _passedColor = Color.gray;
		public enum WidgetStatus
		{
			NotVisited,
			Active,
			CrossActive,
			Visiting,
			Visited,
			Passed
		}
	}
}
