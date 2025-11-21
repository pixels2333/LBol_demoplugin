using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Helpers;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.Core.Stats;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000A2 RID: 162
	public class MapPanel : UiPanel, IInputActionHandler
	{
		// Token: 0x17000166 RID: 358
		// (get) Token: 0x06000873 RID: 2163 RVA: 0x000286D1 File Offset: 0x000268D1
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}

		// Token: 0x17000167 RID: 359
		// (get) Token: 0x06000874 RID: 2164 RVA: 0x000286D4 File Offset: 0x000268D4
		private MapNodeWidget CurrentWidget
		{
			get
			{
				return this._mapNodeWidgets[this._map.VisitingNode.X, this._map.VisitingNode.Y];
			}
		}

		// Token: 0x17000168 RID: 360
		// (get) Token: 0x06000875 RID: 2165 RVA: 0x00028704 File Offset: 0x00026904
		public MapNodeWidget FinalWidget
		{
			get
			{
				MapNode[,] nodes = this._map.Nodes;
				int upperBound = nodes.GetUpperBound(0);
				int upperBound2 = nodes.GetUpperBound(1);
				for (int i = nodes.GetLowerBound(0); i <= upperBound; i++)
				{
					for (int j = nodes.GetLowerBound(1); j <= upperBound2; j++)
					{
						MapNode mapNode = nodes[i, j];
						if (mapNode != null && mapNode.StationType == StationType.Boss)
						{
							return this._mapNodeWidgets[mapNode.X, mapNode.Y];
						}
					}
				}
				return null;
			}
		}

		// Token: 0x06000876 RID: 2166 RVA: 0x0002878C File Offset: 0x0002698C
		public void Awake()
		{
			this.mainHudReturn.onClick.AddListener(new UnityAction(base.Hide));
			foreach (Image image in this.waveImageList)
			{
				int num = this.waveImageList.IndexOf(image);
				image.transform.DOLocalMoveY(40f, 1f, false).SetRelative<TweenerCore<Vector3, Vector3, VectorOptions>>().SetLoops(-1, LoopType.Yoyo)
					.SetEase(Ease.InOutSine)
					.SetDelay((float)num * 0.33f)
					.SetLink(image.gameObject)
					.SetUpdate(true);
			}
			for (int i = 0; i < 3; i++)
			{
				int page = i;
				this.pageWidgetList[i].button.onClick.AddListener(delegate
				{
					this.SetPage(page);
				});
			}
			foreach (Button button in this.pageButtonList)
			{
				button.gameObject.SetActive(false);
			}
			this.enemyInStageTemplate.gameObject.SetActive(false);
			this.enemyInStageButton.onClick.AddListener(new UnityAction(this.OnEnemyInStageButtonClicked));
			this.enemyInStageButton.gameObject.SetActive(false);
			this.RefreshTimer();
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}

		// Token: 0x06000877 RID: 2167 RVA: 0x0002892C File Offset: 0x00026B2C
		private void Update()
		{
			this._timerUpdateInterval -= Time.deltaTime;
			if (this._timerUpdateInterval < 0f)
			{
				this._timerUpdateInterval = 0.5f;
				this.RefreshTimer();
			}
		}

		// Token: 0x06000878 RID: 2168 RVA: 0x00028960 File Offset: 0x00026B60
		private void RefreshTimer()
		{
			int num2;
			int num3;
			int num = Math.DivRem(Math.DivRem(Singleton<GameMaster>.Instance.CurrentGameRunPlayedSeconds, 60, ref num2), 60, ref num3);
			this.playedTimeText.text = string.Format("{0}:{1:00}:{2:00}", num, num3, num2);
		}

		// Token: 0x06000879 RID: 2169 RVA: 0x000289B4 File Offset: 0x00026BB4
		protected override void OnShowing()
		{
			this.UpdateMapNodesStatus();
			foreach (Image image in this.waveImageList)
			{
				image.transform.DOPlay();
			}
			MapNodeWidget[,] mapNodeWidgets = this._mapNodeWidgets;
			int upperBound = mapNodeWidgets.GetUpperBound(0);
			int upperBound2 = mapNodeWidgets.GetUpperBound(1);
			for (int i = mapNodeWidgets.GetLowerBound(0); i <= upperBound; i++)
			{
				for (int j = mapNodeWidgets.GetLowerBound(1); j <= upperBound2; j++)
				{
					MapNodeWidget mapNodeWidget = mapNodeWidgets[i, j];
					if (mapNodeWidget && mapNodeWidget.Status == MapNodeWidget.WidgetStatus.Active)
					{
						mapNodeWidget.StartRootScaleTween();
					}
				}
			}
			AudioManager.PlayUi("MapRoll0", false);
			this.hintText.alpha = 0.9f;
			if (base.GameRun != null)
			{
				this.powerText.text = base.GameRun.Player.Power.ToString() + "/" + base.GameRun.Player.PowerPerLevel.ToString();
				int num = 0;
				num += Enumerable.Sum<StageStats>(base.GameRun.Stats.Stages, (StageStats s) => s.NormalEnemyBluePoint);
				num += Enumerable.Sum<StageStats>(base.GameRun.Stats.Stages, (StageStats s) => s.EliteEnemyBluePoint);
				num += Enumerable.Sum<StageStats>(base.GameRun.Stats.Stages, (StageStats s) => s.BossBluePoint);
				this.bluePointText.text = num.ToString();
				this.moneyText.text = base.GameRun.Money.ToString();
				if (GameMaster.ShowReload)
				{
					string text = "";
					if (base.GameRun.ShowRandomResult)
					{
						text += UiUtils.WrapByColor("StartGame.ShowRandomResult".Localize(true), GlobalConfig.UiRed);
					}
					else
					{
						text += UiUtils.WrapByColor("StartGame.NotShowRandomResult".Localize(true), GlobalConfig.UiRed);
					}
					text = text + "    " + UiUtils.WrapByColor("History.ReloadTimes".LocalizeFormat(new object[] { base.GameRun.ReloadTimes }), GlobalConfig.UiBlue);
					this.historyStatusText.text = text;
					this.historyStatusText.gameObject.SetActive(true);
				}
				else
				{
					this.historyStatusText.gameObject.SetActive(false);
				}
			}
			DOTween.Sequence().AppendInterval(1f).Append(this.hintText.DOFade(0.1f, 0.7f).SetEase(Ease.OutSine))
				.Append(this.hintText.DOFade(0.9f, 0.7f).SetEase(Ease.InSine))
				.SetUpdate(true)
				.SetLoops(-1, LoopType.Restart)
				.SetTarget(this);
			float num2 = this.CurrentWidget.transform.localPosition.x / this.mapScrollRect.content.sizeDelta.x;
			this.mapScrollRect.normalizedPosition = new Vector2(num2, 0f);
			this.ShowEnemyInStage(false);
			this._canvasGroup.interactable = true;
			UiManager.PushActionHandler(this);
		}

		// Token: 0x0600087A RID: 2170 RVA: 0x00028D50 File Offset: 0x00026F50
		protected override void OnShown()
		{
			this.mainHudReturn.interactable = true;
			if (base.GameRun.CurrentStation.Status == StationStatus.Finished)
			{
				this.CurrentWidget.PlayFinish();
			}
		}

		// Token: 0x0600087B RID: 2171 RVA: 0x00028D7C File Offset: 0x00026F7C
		protected override void OnHiding()
		{
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
			this.mainHudReturn.interactable = false;
			AudioManager.PlayUi("MapRoll1", false);
		}

		// Token: 0x0600087C RID: 2172 RVA: 0x00028DA8 File Offset: 0x00026FA8
		protected override void OnHided()
		{
			this.DOKill(true);
			foreach (Image image in this.waveImageList)
			{
				image.transform.DOPause();
			}
			MapNodeWidget[,] mapNodeWidgets = this._mapNodeWidgets;
			int upperBound = mapNodeWidgets.GetUpperBound(0);
			int upperBound2 = mapNodeWidgets.GetUpperBound(1);
			for (int i = mapNodeWidgets.GetLowerBound(0); i <= upperBound; i++)
			{
				for (int j = mapNodeWidgets.GetLowerBound(1); j <= upperBound2; j++)
				{
					MapNodeWidget mapNodeWidget = mapNodeWidgets[i, j];
					if (mapNodeWidget && mapNodeWidget.Status == MapNodeWidget.WidgetStatus.Active)
					{
						mapNodeWidget.EndRootScaleTween();
					}
				}
			}
		}

		// Token: 0x0600087D RID: 2173 RVA: 0x00028E70 File Offset: 0x00027070
		void IInputActionHandler.OnCancel()
		{
			base.Hide();
		}

		// Token: 0x0600087E RID: 2174 RVA: 0x00028E78 File Offset: 0x00027078
		void IInputActionHandler.OnToggleMap()
		{
			base.Hide();
		}

		// Token: 0x0600087F RID: 2175 RVA: 0x00028E80 File Offset: 0x00027080
		private void SetPage(int page)
		{
			base.StartCoroutine(this.CoSetPage(page));
		}

		// Token: 0x06000880 RID: 2176 RVA: 0x00028E90 File Offset: 0x00027090
		public void OnPreviousPageButtonClick(GameObject button)
		{
			this.SetPage(this._page - 1);
			button.transform.DOScale(0.8f, 0.1f).From(1f, true, false).SetLoops(2, LoopType.Yoyo)
				.SetUpdate(true);
		}

		// Token: 0x06000881 RID: 2177 RVA: 0x00028ECF File Offset: 0x000270CF
		public void OnNextPageButtonClick(GameObject button)
		{
			this.SetPage(this._page + 1);
			button.transform.DOScale(0.8f, 0.1f).From(1f, true, false).SetLoops(2, LoopType.Yoyo)
				.SetUpdate(true);
		}

		// Token: 0x06000882 RID: 2178 RVA: 0x00028F0E File Offset: 0x0002710E
		private IEnumerator CoSetPage(int page)
		{
			this._page = page;
			if (this._page < 0)
			{
				this._page = 0;
				yield break;
			}
			if (this._page > 2)
			{
				this._page = 2;
				yield break;
			}
			for (int i = 0; i < 3; i++)
			{
				this.pageWidgetList[i].Animate(i == this._page);
			}
			switch (this._page)
			{
			case 0:
				this.content.DOLocalMoveX(-1450f, 0.1f, false).SetUpdate(true);
				break;
			case 1:
				this.content.DOLocalMoveX(-4450f, 0.1f, false).SetUpdate(true);
				break;
			case 2:
				this.content.DOLocalMoveX(-7600f, 0.1f, false).SetUpdate(true);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield return new WaitForSeconds(0.2f);
			this.pageButtonList[0].gameObject.SetActive(this._page > 0);
			this.pageButtonList[1].gameObject.SetActive(this._page < 2);
			yield break;
		}

		// Token: 0x06000883 RID: 2179 RVA: 0x00028F24 File Offset: 0x00027124
		protected override void OnEnterGameRun()
		{
			base.HandleGameRunEvent<GameEventArgs>(base.GameRun.StageEntered, new GameEventHandler<GameEventArgs>(this.OnStageEntered));
			base.HandleGameRunEvent<StationEventArgs>(base.GameRun.StationFinished, new GameEventHandler<StationEventArgs>(this.OnStationFinished));
		}

		// Token: 0x06000884 RID: 2180 RVA: 0x00028F60 File Offset: 0x00027160
		private void OnStationFinished(StationEventArgs args)
		{
			this.UpdateMapNodesStatus();
		}

		// Token: 0x06000885 RID: 2181 RVA: 0x00028F68 File Offset: 0x00027168
		private void OnStageEntered(GameEventArgs args)
		{
			if (base.GameRun.CurrentMap != null)
			{
				this.GenerateMapUI();
			}
		}

		// Token: 0x06000886 RID: 2182 RVA: 0x00028F7D File Offset: 0x0002717D
		public MapNodeWidget GetMapNodeWidget(int x, int y)
		{
			return this._mapNodeWidgets[x, y];
		}

		// Token: 0x06000887 RID: 2183 RVA: 0x00028F8C File Offset: 0x0002718C
		public void RequestEnterNode(MapNodeWidget enteringWidget)
		{
			base.StartCoroutine(this.RequestEnterNodeRunner(enteringWidget));
		}

		// Token: 0x06000888 RID: 2184 RVA: 0x00028F9C File Offset: 0x0002719C
		private IEnumerator RequestEnterNodeRunner(MapNodeWidget enteringWidget)
		{
			this.EnterNode(enteringWidget);
			yield return new WaitForSeconds(0.5f);
			GameMaster.RequestEnterMapNode(enteringWidget.X, enteringWidget.Y);
			yield break;
		}

		// Token: 0x06000889 RID: 2185 RVA: 0x00028FB4 File Offset: 0x000271B4
		public void EnterNode(MapNodeWidget enteringWidget)
		{
			MapNodeWidget currentWidget = this.CurrentWidget;
			MapNodeWidget[,] mapNodeWidgets = this._mapNodeWidgets;
			int upperBound = mapNodeWidgets.GetUpperBound(0);
			int upperBound2 = mapNodeWidgets.GetUpperBound(1);
			for (int i = mapNodeWidgets.GetLowerBound(0); i <= upperBound; i++)
			{
				for (int j = mapNodeWidgets.GetLowerBound(1); j <= upperBound2; j++)
				{
					MapNodeWidget mapNodeWidget = mapNodeWidgets[i, j];
					if (mapNodeWidget && !(mapNodeWidget == this.CurrentWidget) && mapNodeWidget.Status == MapNodeWidget.WidgetStatus.Active)
					{
						mapNodeWidget.SetButtonInactive();
					}
				}
			}
			MapPanel.EdgeKey edgeKey = new MapPanel.EdgeKey(currentWidget.X, currentWidget.Y, enteringWidget.X, enteringWidget.Y);
			MapLineWidget mapLineWidget;
			if (this._mapLineWidgets.TryGetValue(edgeKey, ref mapLineWidget))
			{
				mapLineWidget.SetLineStatus(MapLineWidget.LineStatus.Acrossing);
				return;
			}
			RandomGen randomGen = new RandomGen(base.GameRun.UISeed);
			for (int k = 0; k < base.GameRun.CurrentStage.Level; k++)
			{
				randomGen.NextULong();
			}
			MapLineWidget mapLineWidget2 = Object.Instantiate<MapLineWidget>(this.lineTemplate, this.lineHolder);
			mapLineWidget2.Initialize(currentWidget, enteringWidget, randomGen);
			this._mapLineWidgets.Add(edgeKey, mapLineWidget2);
			mapLineWidget2.SetLineStatus(MapLineWidget.LineStatus.Acrossing);
			mapLineWidget2.SetLineCrossing();
		}

		// Token: 0x0600088A RID: 2186 RVA: 0x000290F0 File Offset: 0x000272F0
		private void GenerateMapUI()
		{
			foreach (object obj in this.nodeHolder)
			{
				Object.Destroy(((Transform)obj).gameObject);
			}
			foreach (object obj2 in this.lineHolder)
			{
				Object.Destroy(((Transform)obj2).gameObject);
			}
			this._map = base.GameRun.CurrentMap;
			RandomGen randomGen = new RandomGen(base.GameRun.UISeed);
			for (int i = 0; i < base.GameRun.CurrentStage.Level; i++)
			{
				randomGen.NextULong();
			}
			float num = 0f;
			this._mapNodeWidgets = new MapNodeWidget[this._map.Levels, this._map.Width];
			this._mapLineWidgets = new Dictionary<MapPanel.EdgeKey, MapLineWidget>();
			MapNode[,] nodes = this._map.Nodes;
			int num2 = nodes.GetUpperBound(0);
			int num3 = nodes.GetUpperBound(1);
			for (int j = nodes.GetLowerBound(0); j <= num2; j++)
			{
				for (int k = nodes.GetLowerBound(1); k <= num3; k++)
				{
					MapNode mapNode = nodes[j, k];
					if (mapNode != null)
					{
						MapNodeWidget widget = Object.Instantiate<MapNodeWidget>(this.mapNodeTemplate, this.nodeHolder);
						MapNodeWidget.WidgetStatus widgetStatus;
						switch (mapNode.Status)
						{
						case MapNodeStatus.NotVisited:
							widgetStatus = MapNodeWidget.WidgetStatus.NotVisited;
							break;
						case MapNodeStatus.Active:
							widgetStatus = MapNodeWidget.WidgetStatus.Active;
							break;
						case MapNodeStatus.CrossActive:
							widgetStatus = MapNodeWidget.WidgetStatus.CrossActive;
							break;
						case MapNodeStatus.Visiting:
							widgetStatus = MapNodeWidget.WidgetStatus.Visiting;
							break;
						case MapNodeStatus.Visited:
							widgetStatus = MapNodeWidget.WidgetStatus.Visited;
							break;
						case MapNodeStatus.Passed:
							widgetStatus = MapNodeWidget.WidgetStatus.Passed;
							break;
						default:
							throw new ArgumentOutOfRangeException();
						}
						MapNodeWidget.WidgetStatus widgetStatus2 = widgetStatus;
						widget.Initialize(mapNode, widgetStatus2, randomGen, base.GameRun.CurrentStage.Level);
						widget.SetStatus(widgetStatus2);
						widget.Click += delegate
						{
							if (widget.Status == MapNodeWidget.WidgetStatus.Active)
							{
								this.RequestEnterNode(widget);
								widget.SetStatus(MapNodeWidget.WidgetStatus.Visiting);
								return;
							}
							if (widget.Status == MapNodeWidget.WidgetStatus.CrossActive)
							{
								this.RequestEnterNode(widget);
								widget.SetStatus(MapNodeWidget.WidgetStatus.Visiting);
							}
						};
						MapNodeWidget widget2 = widget;
						Vector2 vector;
						switch (this._map.Type)
						{
						case GameMap.UiType.SingleRoute:
							vector = new Vector2((float)mapNode.X * 600f + 800f, (float)mapNode.Y * 350f);
							break;
						case GameMap.UiType.ThreeRoute:
						{
							Vector3 vector2;
							switch (mapNode.Y)
							{
							case 0:
								vector2 = new Vector3((float)mapNode.X * 600f + 800f, 0f);
								break;
							case 1:
								vector2 = new Vector3((float)mapNode.X * 600f + 800f, 350f);
								break;
							case 2:
								vector2 = new Vector3((float)mapNode.X * 600f + 800f, -350f);
								break;
							default:
								throw new ArgumentOutOfRangeException();
							}
							vector = vector2;
							break;
						}
						case GameMap.UiType.NormalGame:
							vector = ((mapNode.Y == 0) ? new Vector3((float)mapNode.X * 600f + 800f, 0f) : new Vector3((float)mapNode.X * 600f + 800f, ((float)mapNode.Y - 2.5f) * 350f));
							break;
						default:
							throw new ArgumentOutOfRangeException();
						}
						widget2.Grid = vector;
						if (this._useRandom)
						{
							widget.Grid = new Vector2(widget.Grid.x + Random.Range(-40f, 40f), widget.Grid.y + Random.Range(-40f, 40f));
						}
						if (mapNode.StationType == StationType.Boss)
						{
							widget.Grid = new Vector2(widget.Grid.x + 400f, widget.Grid.y);
						}
						if (widget.Grid.x > num)
						{
							num = widget.Grid.x;
						}
						this._mapNodeWidgets[mapNode.X, mapNode.Y] = widget;
						SimpleTooltipSource.CreateDirect(widget.gameObject, "", null).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Center).SetDirect(("Map." + widget.MapNode.StationType.ToString()).Localize(true), null);
					}
				}
			}
			this.content.sizeDelta = new Vector2(Mathf.Max(num + 800f, 3840f), this.content.sizeDelta.y);
			MapNodeWidget[,] mapNodeWidgets = this._mapNodeWidgets;
			num3 = mapNodeWidgets.GetUpperBound(0);
			num2 = mapNodeWidgets.GetUpperBound(1);
			for (int j = mapNodeWidgets.GetLowerBound(0); j <= num3; j++)
			{
				for (int k = mapNodeWidgets.GetLowerBound(1); k <= num2; k++)
				{
					MapNodeWidget mapNodeWidget = mapNodeWidgets[j, k];
					if (mapNodeWidget)
					{
						mapNodeWidget.transform.localPosition = mapNodeWidget.Grid;
						if (mapNodeWidget.MapNode.FollowerList.Count > 0)
						{
							foreach (int num4 in mapNodeWidget.MapNode.FollowerList)
							{
								MapLineWidget mapLineWidget = Object.Instantiate<MapLineWidget>(this.lineTemplate, this.lineHolder);
								MapNodeWidget mapNodeWidget2 = this._mapNodeWidgets[mapNodeWidget.MapNode.X + 1, num4];
								mapLineWidget.Initialize(mapNodeWidget, mapNodeWidget2, randomGen);
								this._mapLineWidgets.Add(new MapPanel.EdgeKey(mapNodeWidget.X, mapNodeWidget.Y, mapNodeWidget2.X, mapNodeWidget2.Y), mapLineWidget);
							}
						}
					}
				}
			}
			foreach (ValueTuple<MapNode, MapNode> valueTuple in this._map.Path.Zip(Enumerable.Skip<MapNode>(this._map.Path, 1)))
			{
				MapNode item = valueTuple.Item1;
				MapNode item2 = valueTuple.Item2;
				MapPanel.EdgeKey edgeKey = new MapPanel.EdgeKey(item.X, item.Y, item2.X, item2.Y);
				MapLineWidget mapLineWidget2;
				if (this._mapLineWidgets.TryGetValue(edgeKey, ref mapLineWidget2))
				{
					mapLineWidget2.SetLineStatus(MapLineWidget.LineStatus.Acrossed);
				}
				else
				{
					for (int l = 0; l < base.GameRun.CurrentStage.Level; l++)
					{
						randomGen.NextULong();
					}
					MapLineWidget mapLineWidget3 = Object.Instantiate<MapLineWidget>(this.lineTemplate, this.lineHolder);
					mapLineWidget3.Initialize(this._mapNodeWidgets[item.X, item.Y], this._mapNodeWidgets[item2.X, item2.Y], randomGen);
					this._mapLineWidgets.Add(edgeKey, mapLineWidget3);
					mapLineWidget3.SetLineStatus(MapLineWidget.LineStatus.Acrossing);
					mapLineWidget3.SetLineCrossing();
				}
			}
			this.SetEnemyInStage(base.GameRun.CurrentStage);
		}

		// Token: 0x0600088B RID: 2187 RVA: 0x000298A4 File Offset: 0x00027AA4
		private void UpdateMapNodesStatus()
		{
			if (this._map != base.GameRun.CurrentMap)
			{
				this.GenerateMapUI();
				return;
			}
			if (this._map == null)
			{
				return;
			}
			MapNodeWidget[,] mapNodeWidgets = this._mapNodeWidgets;
			int upperBound = mapNodeWidgets.GetUpperBound(0);
			int upperBound2 = mapNodeWidgets.GetUpperBound(1);
			for (int i = mapNodeWidgets.GetLowerBound(0); i <= upperBound; i++)
			{
				for (int j = mapNodeWidgets.GetLowerBound(1); j <= upperBound2; j++)
				{
					MapNodeWidget mapNodeWidget = mapNodeWidgets[i, j];
					if (mapNodeWidget != null)
					{
						MapNode mapNode = this._map.Nodes[mapNodeWidget.X, mapNodeWidget.Y];
						mapNodeWidget.SetStatus(mapNode);
					}
				}
			}
			foreach (MapLineWidget mapLineWidget in this._mapLineWidgets.Values)
			{
				if ((mapLineWidget.SourceX < this.CurrentWidget.X && mapLineWidget.Status == MapLineWidget.LineStatus.NotAcrossed) || mapLineWidget.Status == MapLineWidget.LineStatus.Active)
				{
					mapLineWidget.SetLineStatus(MapLineWidget.LineStatus.Passed);
				}
			}
		}

		// Token: 0x0600088C RID: 2188 RVA: 0x000299C0 File Offset: 0x00027BC0
		private void OnEnemyInStageButtonClicked()
		{
			this.ShowEnemyInStage(!this.enemyInStageRoot.gameObject.activeSelf);
		}

		// Token: 0x0600088D RID: 2189 RVA: 0x000299DB File Offset: 0x00027BDB
		private void ShowEnemyInStage(bool on)
		{
			this.enemyInStageRoot.gameObject.SetActive(on);
		}

		// Token: 0x0600088E RID: 2190 RVA: 0x000299F0 File Offset: 0x00027BF0
		private void SetEnemyInStage(Stage stage)
		{
			foreach (object obj in this.enemyInsStageLayout)
			{
				Object.Destroy(((Transform)obj).gameObject);
			}
			this.<SetEnemyInStage>g__CreateWidget|75_1("第一节敌人", true);
			foreach (RandomPoolEntry<string> randomPoolEntry in stage.EnemyPoolAct1)
			{
				this.<SetEnemyInStage>g__CreateWidgetForEnemy|75_0(randomPoolEntry.Elem);
			}
			this.<SetEnemyInStage>g__CreateWidget|75_1("第二节敌人", true);
			foreach (RandomPoolEntry<string> randomPoolEntry2 in stage.EnemyPoolAct2)
			{
				this.<SetEnemyInStage>g__CreateWidgetForEnemy|75_0(randomPoolEntry2.Elem);
			}
			this.<SetEnemyInStage>g__CreateWidget|75_1("第三节敌人", true);
			foreach (RandomPoolEntry<string> randomPoolEntry3 in stage.EnemyPoolAct3)
			{
				this.<SetEnemyInStage>g__CreateWidgetForEnemy|75_0(randomPoolEntry3.Elem);
			}
			this.<SetEnemyInStage>g__CreateWidget|75_1("精英敌人", true);
			foreach (RandomPoolEntry<string> randomPoolEntry4 in stage.EliteEnemyPool)
			{
				this.<SetEnemyInStage>g__CreateWidgetForEnemy|75_0(randomPoolEntry4.Elem);
			}
			this.<SetEnemyInStage>g__CreateWidget|75_1("Boss", true);
			foreach (RandomPoolEntry<string> randomPoolEntry5 in stage.BossPool)
			{
				this.<SetEnemyInStage>g__CreateWidgetForEnemy|75_0(randomPoolEntry5.Elem);
			}
			this.<SetEnemyInStage>g__CreateWidget|75_1("冒险", true);
			foreach (RandomPoolEntry<Type> randomPoolEntry6 in stage.AdventurePool)
			{
				Adventure adventure = Library.CreateAdventure(randomPoolEntry6.Elem.Name);
				this.<SetEnemyInStage>g__CreateWidget|75_1(adventure.HostName + ": " + adventure.Title, false);
			}
		}

		// Token: 0x06000891 RID: 2193 RVA: 0x00029C88 File Offset: 0x00027E88
		[CompilerGenerated]
		private void <SetEnemyInStage>g__CreateWidgetForEnemy|75_0(string enemyGroupId)
		{
			EnemyGroupConfig enemyGroupConfig = EnemyGroupConfig.FromId(enemyGroupId);
			this.<SetEnemyInStage>g__CreateWidget|75_1((enemyGroupConfig != null) ? enemyGroupConfig.Name : enemyGroupId, false);
		}

		// Token: 0x06000892 RID: 2194 RVA: 0x00029CB0 File Offset: 0x00027EB0
		[CompilerGenerated]
		private void <SetEnemyInStage>g__CreateWidget|75_1(string text, bool isTitle = false)
		{
			Image image = Object.Instantiate<Image>(this.enemyInStageTemplate, this.enemyInsStageLayout);
			TextMeshProUGUI component = image.transform.Find("Text").GetComponent<TextMeshProUGUI>();
			if (isTitle)
			{
				image.color = MapPanel.Title;
			}
			if (isTitle)
			{
				component.text = "——" + text;
			}
			else
			{
				component.text = text;
			}
			image.gameObject.SetActive(true);
		}

		// Token: 0x040005FF RID: 1535
		public const float MapLineAcrossTime = 0.5f;

		// Token: 0x04000600 RID: 1536
		[SerializeField]
		private ScrollRect mapScrollRect;

		// Token: 0x04000601 RID: 1537
		[SerializeField]
		private MapNodeWidget mapNodeTemplate;

		// Token: 0x04000602 RID: 1538
		[SerializeField]
		private MapLineWidget lineTemplate;

		// Token: 0x04000603 RID: 1539
		[SerializeField]
		private RectTransform content;

		// Token: 0x04000604 RID: 1540
		[SerializeField]
		private RectTransform lineHolder;

		// Token: 0x04000605 RID: 1541
		[SerializeField]
		private RectTransform nodeHolder;

		// Token: 0x04000606 RID: 1542
		[SerializeField]
		private Button mainHudReturn;

		// Token: 0x04000607 RID: 1543
		[SerializeField]
		private TextMeshProUGUI hintText;

		// Token: 0x04000608 RID: 1544
		[SerializeField]
		private TextMeshProUGUI powerText;

		// Token: 0x04000609 RID: 1545
		[SerializeField]
		private TextMeshProUGUI bluePointText;

		// Token: 0x0400060A RID: 1546
		[SerializeField]
		private TextMeshProUGUI moneyText;

		// Token: 0x0400060B RID: 1547
		[SerializeField]
		private TextMeshProUGUI playedTimeText;

		// Token: 0x0400060C RID: 1548
		[SerializeField]
		private TextMeshProUGUI historyStatusText;

		// Token: 0x0400060D RID: 1549
		[SerializeField]
		private List<MapPageChangeWidget> pageWidgetList;

		// Token: 0x0400060E RID: 1550
		[SerializeField]
		private List<Button> pageButtonList;

		// Token: 0x0400060F RID: 1551
		[SerializeField]
		private List<Image> waveImageList;

		// Token: 0x04000610 RID: 1552
		[Header("Enemy In Stage")]
		[SerializeField]
		private Button enemyInStageButton;

		// Token: 0x04000611 RID: 1553
		[SerializeField]
		private Transform enemyInStageRoot;

		// Token: 0x04000612 RID: 1554
		[SerializeField]
		private Image enemyInStageTemplate;

		// Token: 0x04000613 RID: 1555
		[SerializeField]
		private Transform enemyInsStageLayout;

		// Token: 0x04000614 RID: 1556
		private CanvasGroup _canvasGroup;

		// Token: 0x04000615 RID: 1557
		private int _page;

		// Token: 0x04000616 RID: 1558
		private GameMap _map;

		// Token: 0x04000617 RID: 1559
		private MapNodeWidget[,] _mapNodeWidgets;

		// Token: 0x04000618 RID: 1560
		private Dictionary<MapPanel.EdgeKey, MapLineWidget> _mapLineWidgets;

		// Token: 0x04000619 RID: 1561
		private float _timerUpdateInterval = 0.5f;

		// Token: 0x0400061A RID: 1562
		private const float MaxFade = 0.9f;

		// Token: 0x0400061B RID: 1563
		private const float MinFade = 0.1f;

		// Token: 0x0400061C RID: 1564
		private const float MaxPauseTime = 1f;

		// Token: 0x0400061D RID: 1565
		private const float FadeToMinTime = 0.7f;

		// Token: 0x0400061E RID: 1566
		private const float MinPauseTime = 0.2f;

		// Token: 0x0400061F RID: 1567
		private const float FadeToMaxTime = 0.7f;

		// Token: 0x04000620 RID: 1568
		private const float Page1 = -1450f;

		// Token: 0x04000621 RID: 1569
		private const float Page2 = -4450f;

		// Token: 0x04000622 RID: 1570
		private const float Page3 = -7600f;

		// Token: 0x04000623 RID: 1571
		private const float XInterval = 600f;

		// Token: 0x04000624 RID: 1572
		private const float YInterval = 350f;

		// Token: 0x04000625 RID: 1573
		private const float XEdge = 800f;

		// Token: 0x04000626 RID: 1574
		private const float BossAddInterval = 400f;

		// Token: 0x04000627 RID: 1575
		private const float XRandom = 40f;

		// Token: 0x04000628 RID: 1576
		private const float YRandom = 40f;

		// Token: 0x04000629 RID: 1577
		private bool _useRandom = true;

		// Token: 0x0400062A RID: 1578
		private static readonly Color Title = new Color32(107, 147, 194, byte.MaxValue);

		// Token: 0x0200026E RID: 622
		private struct EdgeKey
		{
			// Token: 0x06001585 RID: 5509 RVA: 0x0006295C File Offset: 0x00060B5C
			public EdgeKey(int sourceX, int sourceY, int targetX, int targetY)
			{
				this.SourceX = sourceX;
				this.SourceY = sourceY;
				this.TargetX = targetX;
				this.TargetY = targetY;
			}

			// Token: 0x06001586 RID: 5510 RVA: 0x00062990 File Offset: 0x00060B90
			public override string ToString()
			{
				return string.Format("({0}, {1}, {2}, {3})", new object[] { this.SourceX, this.SourceY, this.TargetX, this.TargetY });
			}

			// Token: 0x040010F1 RID: 4337
			public int SourceX;

			// Token: 0x040010F2 RID: 4338
			public int SourceY;

			// Token: 0x040010F3 RID: 4339
			public int TargetX;

			// Token: 0x040010F4 RID: 4340
			public int TargetY;
		}
	}
}
