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
	// Token: 0x02000063 RID: 99
	public class MapNodeWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		// Token: 0x170000EB RID: 235
		// (get) Token: 0x06000561 RID: 1377 RVA: 0x00017435 File Offset: 0x00015635
		// (set) Token: 0x06000562 RID: 1378 RVA: 0x0001743D File Offset: 0x0001563D
		public Vector2 Grid { get; set; }

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x06000563 RID: 1379 RVA: 0x00017446 File Offset: 0x00015646
		// (set) Token: 0x06000564 RID: 1380 RVA: 0x0001744E File Offset: 0x0001564E
		public MapNode MapNode { get; private set; }

		// Token: 0x170000ED RID: 237
		// (get) Token: 0x06000565 RID: 1381 RVA: 0x00017457 File Offset: 0x00015657
		// (set) Token: 0x06000566 RID: 1382 RVA: 0x0001745F File Offset: 0x0001565F
		public MapNodeWidget.WidgetStatus Status { get; private set; }

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x06000567 RID: 1383 RVA: 0x00017468 File Offset: 0x00015668
		// (set) Token: 0x06000568 RID: 1384 RVA: 0x00017470 File Offset: 0x00015670
		private bool ButtonActive { get; set; }

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x06000569 RID: 1385 RVA: 0x00017479 File Offset: 0x00015679
		public int X
		{
			get
			{
				return this.MapNode.X;
			}
		}

		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x0600056A RID: 1386 RVA: 0x00017486 File Offset: 0x00015686
		public int Y
		{
			get
			{
				return this.MapNode.Y;
			}
		}

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x0600056B RID: 1387 RVA: 0x00017493 File Offset: 0x00015693
		// (set) Token: 0x0600056C RID: 1388 RVA: 0x0001749B File Offset: 0x0001569B
		public int StageLevel { get; set; }

		// Token: 0x0600056D RID: 1389 RVA: 0x000174A4 File Offset: 0x000156A4
		private void Awake()
		{
			this.button.onClick.AddListener(new UnityAction(this.OnButtonClicked));
		}

		// Token: 0x0600056E RID: 1390 RVA: 0x000174C4 File Offset: 0x000156C4
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

		// Token: 0x0600056F RID: 1391 RVA: 0x00017708 File Offset: 0x00015908
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

		// Token: 0x06000570 RID: 1392 RVA: 0x00017774 File Offset: 0x00015974
		private void ConvertToBossWidget()
		{
			base.GetComponent<RectTransform>().sizeDelta = new Vector2(800f, 800f);
			this.baseImage.sprite = this.bossBase;
			this.iconImage.sprite = this.bossUnselect;
			this.outerImage.sprite = this.bossOuter;
		}

		// Token: 0x06000571 RID: 1393 RVA: 0x000177CE File Offset: 0x000159CE
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

		// Token: 0x14000007 RID: 7
		// (add) Token: 0x06000572 RID: 1394 RVA: 0x000177FC File Offset: 0x000159FC
		// (remove) Token: 0x06000573 RID: 1395 RVA: 0x00017834 File Offset: 0x00015A34
		public event Action Click;

		// Token: 0x06000574 RID: 1396 RVA: 0x0001786C File Offset: 0x00015A6C
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

		// Token: 0x06000575 RID: 1397 RVA: 0x00017938 File Offset: 0x00015B38
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

		// Token: 0x06000576 RID: 1398 RVA: 0x000179A1 File Offset: 0x00015BA1
		public void SetButtonInactive()
		{
			this.ButtonActive = false;
		}

		// Token: 0x06000577 RID: 1399 RVA: 0x000179AA File Offset: 0x00015BAA
		public void PlayFinish()
		{
			if (this.signalImage.fillAmount < 1f)
			{
				this.signalImage.DOFillAmount(1f, 0.5f).SetAutoKill<TweenerCore<float, float, FloatOptions>>().SetUpdate(true);
			}
		}

		// Token: 0x06000578 RID: 1400 RVA: 0x000179E0 File Offset: 0x00015BE0
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

		// Token: 0x06000579 RID: 1401 RVA: 0x00017A58 File Offset: 0x00015C58
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

		// Token: 0x0600057A RID: 1402 RVA: 0x00017ADC File Offset: 0x00015CDC
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

		// Token: 0x0600057B RID: 1403 RVA: 0x00017B7E File Offset: 0x00015D7E
		public void EndRootScaleTween()
		{
			this.root.DOKill(false);
			this.root.localScale = Vector3.one;
		}

		// Token: 0x0600057C RID: 1404 RVA: 0x00017B9D File Offset: 0x00015D9D
		public void SetBoss(string bossId)
		{
			this.iconImage.sprite = ResourcesHelper.TryGetBossIcon(bossId);
		}

		// Token: 0x04000322 RID: 802
		[SerializeField]
		private AssociationList<StationType, Sprite> iconTable;

		// Token: 0x04000323 RID: 803
		[SerializeField]
		private List<Sprite> tradeSprites;

		// Token: 0x04000324 RID: 804
		[SerializeField]
		private List<Sprite> normalEnemySprites1;

		// Token: 0x04000325 RID: 805
		[SerializeField]
		private List<Sprite> normalEnemySprites2;

		// Token: 0x04000326 RID: 806
		[SerializeField]
		private List<Sprite> normalEnemySprites3;

		// Token: 0x04000327 RID: 807
		[SerializeField]
		private List<Sprite> eliteEnemySprites;

		// Token: 0x04000328 RID: 808
		[SerializeField]
		private Sprite defaultBase;

		// Token: 0x04000329 RID: 809
		[SerializeField]
		private Sprite colorBase;

		// Token: 0x0400032A RID: 810
		[SerializeField]
		private Sprite bossBase;

		// Token: 0x0400032B RID: 811
		[SerializeField]
		private Sprite bossOuter;

		// Token: 0x0400032C RID: 812
		[SerializeField]
		private Sprite bossUnselect;

		// Token: 0x0400032D RID: 813
		[SerializeField]
		private TextMeshProUGUI bossName;

		// Token: 0x0400032E RID: 814
		[SerializeField]
		private Button button;

		// Token: 0x0400032F RID: 815
		[SerializeField]
		private Transform root;

		// Token: 0x04000330 RID: 816
		[SerializeField]
		private Image baseImage;

		// Token: 0x04000331 RID: 817
		[SerializeField]
		private Image outerImage;

		// Token: 0x04000332 RID: 818
		[SerializeField]
		private Image crossingOuterImage;

		// Token: 0x04000333 RID: 819
		[SerializeField]
		private Image iconImage;

		// Token: 0x04000334 RID: 820
		[SerializeField]
		private Image signalImage;

		// Token: 0x0400033A RID: 826
		private readonly Color _passedColor = Color.gray;

		// Token: 0x020001D9 RID: 473
		public enum WidgetStatus
		{
			// Token: 0x04000F23 RID: 3875
			NotVisited,
			// Token: 0x04000F24 RID: 3876
			Active,
			// Token: 0x04000F25 RID: 3877
			CrossActive,
			// Token: 0x04000F26 RID: 3878
			Visiting,
			// Token: 0x04000F27 RID: 3879
			Visited,
			// Token: 0x04000F28 RID: 3880
			Passed
		}
	}
}
