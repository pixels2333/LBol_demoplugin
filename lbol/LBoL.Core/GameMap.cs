using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core.Stations;
using UnityEngine;

namespace LBoL.Core
{
	// Token: 0x02000040 RID: 64
	public class GameMap
	{
		// Token: 0x060001DC RID: 476 RVA: 0x00004C3B File Offset: 0x00002E3B
		private GameMap(int maxLevels, int maxLevelNotes, GameMap.UiType type, [CanBeNull] string bossId)
		{
			this.Type = type;
			this.BossId = bossId;
			this.Nodes = new MapNode[maxLevels, maxLevelNotes];
		}

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x060001DD RID: 477 RVA: 0x00004C6A File Offset: 0x00002E6A
		public MapNode[,] Nodes { get; }

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x060001DE RID: 478 RVA: 0x00004C72 File Offset: 0x00002E72
		public IEnumerable<MapNode> AllNodes
		{
			get
			{
				return Enumerable.Where<MapNode>(Enumerable.Cast<MapNode>(this.Nodes), (MapNode node) => node != null);
			}
		}

		// Token: 0x1700009B RID: 155
		// (get) Token: 0x060001DF RID: 479 RVA: 0x00004CA3 File Offset: 0x00002EA3
		// (set) Token: 0x060001E0 RID: 480 RVA: 0x00004CAB File Offset: 0x00002EAB
		public MapNode StartNode { get; private set; }

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x060001E1 RID: 481 RVA: 0x00004CB4 File Offset: 0x00002EB4
		// (set) Token: 0x060001E2 RID: 482 RVA: 0x00004CBC File Offset: 0x00002EBC
		public MapNode EndNode { get; private set; }

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x060001E3 RID: 483 RVA: 0x00004CC5 File Offset: 0x00002EC5
		// (set) Token: 0x060001E4 RID: 484 RVA: 0x00004CCD File Offset: 0x00002ECD
		public MapNode BossNode { get; private set; }

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x060001E5 RID: 485 RVA: 0x00004CD6 File Offset: 0x00002ED6
		public GameMap.UiType Type { get; }

		// Token: 0x1700009F RID: 159
		// (get) Token: 0x060001E6 RID: 486 RVA: 0x00004CDE File Offset: 0x00002EDE
		public string BossId { get; }

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x060001E7 RID: 487 RVA: 0x00004CE6 File Offset: 0x00002EE6
		public int Levels
		{
			get
			{
				return this.Nodes.GetLength(0);
			}
		}

		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x060001E8 RID: 488 RVA: 0x00004CF4 File Offset: 0x00002EF4
		public int Width
		{
			get
			{
				return this.Nodes.GetLength(1);
			}
		}

		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x060001E9 RID: 489 RVA: 0x00004D02 File Offset: 0x00002F02
		// (set) Token: 0x060001EA RID: 490 RVA: 0x00004D0A File Offset: 0x00002F0A
		public MapNode VisitingNode { get; private set; }

		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x060001EB RID: 491 RVA: 0x00004D13 File Offset: 0x00002F13
		public IReadOnlyList<MapNode> Path
		{
			get
			{
				return this._path.AsReadOnly();
			}
		}

		// Token: 0x060001EC RID: 492 RVA: 0x00004D20 File Offset: 0x00002F20
		public void EnterNode(MapNode node, bool freeMove, bool forced)
		{
			if (!forced && node.Status != MapNodeStatus.Active && node.Status != MapNodeStatus.CrossActive)
			{
				throw new InvalidOperationException(string.Format("Entering MapNode ({0}, {1}) status {2} != {3}", new object[] { node.X, node.Y, node.Status, "Active" }));
			}
			if (this.VisitingNode != null)
			{
				if (this.VisitingNode.Status != MapNodeStatus.Visiting)
				{
					throw new InvalidOperationException(string.Format("Leaving MapNode({0}, {1}) is visiting", node.X, node.Y));
				}
				if (!forced && !freeMove && !this.VisitingNode.FollowerList.Contains(node.Y))
				{
					throw new InvalidOperationException(string.Format("Entering MapNode({0}, {1}) is not neighbor of MapNode({2}, {3})", new object[]
					{
						node.X,
						node.Y,
						this.VisitingNode.X,
						this.VisitingNode.Y
					}));
				}
				this.VisitingNode.Status = MapNodeStatus.Visited;
			}
			node.Status = MapNodeStatus.Visiting;
			MapNode visitingNode = this.VisitingNode;
			int num = ((visitingNode != null) ? visitingNode.X : 0);
			int x = node.X;
			for (int i = num; i < x; i++)
			{
				int j = 0;
				while (j < this.Width)
				{
					MapNode mapNode = this.Nodes[i, j];
					if (mapNode == null)
					{
						goto IL_0168;
					}
					MapNodeStatus status = mapNode.Status;
					if (status > MapNodeStatus.CrossActive)
					{
						goto IL_0168;
					}
					bool flag = true;
					IL_016B:
					if (flag)
					{
						mapNode.Status = MapNodeStatus.Passed;
					}
					j++;
					continue;
					IL_0168:
					flag = false;
					goto IL_016B;
				}
			}
			for (int k = 0; k < this.Width; k++)
			{
				if (k != node.Y)
				{
					MapNode mapNode2 = this.Nodes[x, k];
					if (mapNode2 != null)
					{
						mapNode2.Status = MapNodeStatus.Passed;
					}
				}
			}
			this.VisitingNode = node;
			this._path.Add(node);
		}

		// Token: 0x060001ED RID: 493 RVA: 0x00004F08 File Offset: 0x00003108
		internal void SetAdjacentNodesStatus(GameRunMapMode mapMode)
		{
			if (this.VisitingNode == null || this.VisitingNode == this.EndNode)
			{
				return;
			}
			if (mapMode == GameRunMapMode.Normal)
			{
				int num = this.VisitingNode.X + 1;
				using (List<int>.Enumerator enumerator = this.VisitingNode.FollowerList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						int num2 = enumerator.Current;
						this.Nodes[num, num2].Status = MapNodeStatus.Active;
					}
					return;
				}
			}
			if (mapMode == GameRunMapMode.Crossing)
			{
				int num3 = this.VisitingNode.X + 1;
				foreach (int num4 in this.VisitingNode.FollowerList)
				{
					this.Nodes[num3, num4].Status = MapNodeStatus.Active;
				}
				for (int i = 0; i < this.Width; i++)
				{
					MapNode mapNode = this.Nodes[num3, i];
					if (mapNode != null && mapNode.Status == MapNodeStatus.NotVisited)
					{
						mapNode.Status = MapNodeStatus.CrossActive;
					}
				}
				return;
			}
			if (mapMode == GameRunMapMode.TeleportBoss)
			{
				this.EndNode.Status = MapNodeStatus.Active;
				return;
			}
			if (mapMode == GameRunMapMode.Free)
			{
				for (int j = this.VisitingNode.X + 1; j < this.Levels; j++)
				{
					for (int k = 0; k < this.Width; k++)
					{
						MapNode mapNode2 = this.Nodes[j, k];
						if (mapNode2 != null)
						{
							mapNode2.Status = MapNodeStatus.Active;
						}
					}
				}
			}
		}

		// Token: 0x060001EE RID: 494 RVA: 0x000050A4 File Offset: 0x000032A4
		public void RestorePath(IEnumerable<MapNode> nodes)
		{
			foreach (MapNode mapNode in nodes)
			{
				mapNode.Status = MapNodeStatus.Active;
				this.EnterNode(mapNode, true, true);
			}
		}

		// Token: 0x060001EF RID: 495 RVA: 0x000050F8 File Offset: 0x000032F8
		public static GameMap CreateSingleRoute([CanBeNull] string bossId, params StationType[] types)
		{
			GameMap gameMap = new GameMap(types.Length + 1, 1, GameMap.UiType.SingleRoute, bossId);
			MapNode[,] nodes = gameMap.Nodes;
			int num = 0;
			int num2 = 0;
			MapNode mapNode = new MapNode(gameMap, 0, 0, 1);
			mapNode.StationType = StationType.Entry;
			mapNode.FollowerList.Add(0);
			nodes[num, num2] = mapNode;
			for (int i = 0; i < types.Length - 1; i++)
			{
				StationType stationType = types[i];
				if (stationType == StationType.Boss)
				{
					Debug.LogWarning("Creating single-route map with non-ending boss node");
				}
				MapNode[,] nodes2 = gameMap.Nodes;
				int num3 = i + 1;
				int num4 = 0;
				MapNode mapNode2 = new MapNode(gameMap, i + 1, 0, 1);
				mapNode2.StationType = stationType;
				mapNode2.FollowerList.Add(0);
				nodes2[num3, num4] = mapNode2;
			}
			gameMap.Nodes[types.Length, 0] = new MapNode(gameMap, types.Length, 0, 1)
			{
				StationType = Enumerable.Last<StationType>(types)
			};
			gameMap.StartNode = gameMap.Nodes[0, 0];
			gameMap.StartNode.Status = MapNodeStatus.Active;
			gameMap.EndNode = gameMap.Nodes[types.Length, 0];
			if (gameMap.EndNode.StationType == StationType.Boss)
			{
				gameMap.BossNode = gameMap.EndNode;
			}
			return gameMap;
		}

		// Token: 0x060001F0 RID: 496 RVA: 0x00005204 File Offset: 0x00003404
		public static GameMap CreateThreeRoute([CanBeNull] string bossId, params StationType[] types)
		{
			int num = Mathf.CeilToInt((float)types.Length / 3f);
			GameMap gameMap = new GameMap(num + 2, 3, GameMap.UiType.ThreeRoute, bossId);
			gameMap.Nodes[0, 0] = new MapNode(gameMap, 0, 0, 1)
			{
				StationType = StationType.Entry
			};
			for (int i = 1; i < num + 1; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					StationType stationType = types.TryGetValue(i * 3 + j - 3);
					gameMap.Nodes[i, j] = new MapNode(gameMap, i, j, 1)
					{
						StationType = stationType
					};
				}
			}
			gameMap.Nodes[num + 1, 0] = new MapNode(gameMap, num + 1, 0, 1)
			{
				StationType = StationType.Boss
			};
			foreach (MapNode mapNode in gameMap.AllNodes)
			{
				foreach (MapNode mapNode2 in gameMap.AllNodes)
				{
					if (mapNode2.X == mapNode.X + 1)
					{
						mapNode.FollowerList.Add(mapNode2.Y);
					}
				}
			}
			gameMap.StartNode = gameMap.Nodes[0, 0];
			gameMap.StartNode.Status = MapNodeStatus.Active;
			gameMap.EndNode = gameMap.Nodes[num + 1, 0];
			if (gameMap.EndNode.StationType == StationType.Boss)
			{
				gameMap.BossNode = gameMap.EndNode;
			}
			return gameMap;
		}

		// Token: 0x060001F1 RID: 497 RVA: 0x000053A4 File Offset: 0x000035A4
		public static GameMap CreateFourRoute([CanBeNull] string bossId, params StationType[] types)
		{
			int num = Mathf.CeilToInt((float)types.Length / 4f);
			GameMap gameMap = new GameMap(num + 2, 5, GameMap.UiType.NormalGame, bossId);
			gameMap.Nodes[0, 0] = new MapNode(gameMap, 0, 0, 1)
			{
				StationType = StationType.Entry
			};
			for (int i = 1; i < num + 1; i++)
			{
				for (int j = 1; j < 5; j++)
				{
					StationType stationType = types.TryGetValue(i * 4 + j - 5);
					if (stationType == StationType.None)
					{
						stationType = StationType.Adventure;
					}
					gameMap.Nodes[i, j] = new MapNode(gameMap, i, j, 1)
					{
						StationType = stationType
					};
				}
			}
			gameMap.Nodes[num + 1, 0] = new MapNode(gameMap, num + 1, 0, 1)
			{
				StationType = StationType.Boss
			};
			foreach (MapNode mapNode in gameMap.AllNodes)
			{
				foreach (MapNode mapNode2 in gameMap.AllNodes)
				{
					if (mapNode2.X == mapNode.X + 1)
					{
						mapNode.FollowerList.Add(mapNode2.Y);
					}
				}
			}
			gameMap.StartNode = gameMap.Nodes[0, 0];
			gameMap.StartNode.Status = MapNodeStatus.Active;
			gameMap.EndNode = gameMap.Nodes[num + 1, 0];
			if (gameMap.EndNode.StationType == StationType.Boss)
			{
				gameMap.BossNode = gameMap.EndNode;
			}
			return gameMap;
		}

		// Token: 0x060001F2 RID: 498 RVA: 0x0000554C File Offset: 0x0000374C
		public static GameMap CreateNormalMap(RandomGen rng, [CanBeNull] string bossId, bool isSelectingBoss)
		{
			GameMap gameMap = new GameMap(17, 5, GameMap.UiType.NormalGame, bossId);
			gameMap.Nodes[0, 0] = new MapNode(gameMap, 0, 0, 1)
			{
				StationType = StationType.Entry
			};
			gameMap.Nodes[5, 0] = new MapNode(gameMap, 5, 0, 1)
			{
				StationType = (isSelectingBoss ? StationType.Select : StationType.Trade)
			};
			gameMap.Nodes[10, 0] = new MapNode(gameMap, 10, 0, 2)
			{
				StationType = StationType.Supply
			};
			gameMap.Nodes[15, 0] = new MapNode(gameMap, 15, 0, 3)
			{
				StationType = StationType.Gap
			};
			gameMap.Nodes[16, 0] = new MapNode(gameMap, 16, 0, 3)
			{
				StationType = StationType.Boss
			};
			for (int i = 1; i < 5; i++)
			{
				gameMap.Nodes[0, 0].FollowerList.Add(i);
				gameMap.Nodes[5, 0].FollowerList.Add(i);
				gameMap.Nodes[10, 0].FollowerList.Add(i);
			}
			gameMap.Nodes[15, 0].FollowerList.Add(0);
			StationType[,] array = GameMap.GetAct1(rng);
			for (int j = 1; j < 5; j++)
			{
				for (int k = 1; k < 5; k++)
				{
					gameMap.Nodes[j, k] = new MapNode(gameMap, j, k, 1)
					{
						StationType = array[j - 1, k - 1]
					};
					gameMap.Nodes[j, k].FollowerList.Add((j == 4) ? 0 : k);
				}
			}
			array = GameMap.GetAct2(rng);
			for (int l = 6; l < 10; l++)
			{
				for (int m = 1; m < 5; m++)
				{
					gameMap.Nodes[l, m] = new MapNode(gameMap, l, m, 2)
					{
						StationType = array[l - 6, m - 1]
					};
					gameMap.Nodes[l, m].FollowerList.Add((l == 9) ? 0 : m);
				}
			}
			array = GameMap.GetAct3(rng);
			for (int n = 11; n < 15; n++)
			{
				for (int num = 1; num < 5; num++)
				{
					gameMap.Nodes[n, num] = new MapNode(gameMap, n, num, 3)
					{
						StationType = array[n - 11, num - 1]
					};
					gameMap.Nodes[n, num].FollowerList.Add((n == 14) ? 0 : num);
				}
			}
			GameMap.CreateSideWays(gameMap, rng, 1, 5);
			GameMap.CreateSideWays(gameMap, rng, 6, 10);
			GameMap.CreateSideWays(gameMap, rng, 11, 15);
			gameMap.StartNode = gameMap.Nodes[0, 0];
			gameMap.StartNode.Status = MapNodeStatus.Active;
			gameMap.BossNode = (gameMap.EndNode = gameMap.Nodes[16, 0]);
			return gameMap;
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x00005834 File Offset: 0x00003A34
		private static StationType[,] GetAct1(RandomGen rng)
		{
			StationType[,] array = new StationType[4, 4];
			List<int> list = new List<int>();
			list.Add(1);
			list.Add(2);
			list.Add(3);
			list.Add(4);
			list.Add(5);
			List<int> list2 = list;
			list2.Shuffle(rng);
			for (int i = 0; i < 4; i++)
			{
				array[0, i] = StationType.Enemy;
				switch (list2[i])
				{
				case 1:
				case 2:
					array[1, i] = StationType.Enemy;
					array[2, i] = StationType.Adventure;
					break;
				case 3:
				case 4:
					array[1, i] = StationType.Adventure;
					array[2, i] = StationType.Enemy;
					break;
				case 5:
					array[1, i] = StationType.Enemy;
					array[2, i] = StationType.Enemy;
					break;
				}
			}
			List<StationType> list3 = new List<StationType>();
			list3.Add(StationType.EliteEnemy);
			list3.Add(StationType.Gap);
			list3.Add(StationType.Shop);
			List<StationType> list4 = list3;
			list4.Shuffle(rng);
			float num = rng.NextFloat(0f, 1f);
			if (num >= 0.6f)
			{
				if (num >= 0.8f)
				{
					list4.Add(StationType.Shop);
				}
				else
				{
					list4.Add(StationType.Gap);
				}
			}
			else
			{
				list4.Add(StationType.EliteEnemy);
			}
			for (int j = 0; j < 4; j++)
			{
				array[3, j] = list4[j];
			}
			return array;
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x0000597C File Offset: 0x00003B7C
		private static StationType[,] GetAct2(RandomGen rng)
		{
			StationType[,] array = new StationType[4, 4];
			List<int> list = new List<int>();
			list.Add(1);
			list.Add(2);
			list.Add(3);
			list.Add(4);
			list.Add(5);
			List<int> list2 = list;
			list2.Shuffle(rng);
			for (int i = 0; i < 4; i++)
			{
				switch (list2[i])
				{
				case 1:
				case 2:
					array[0, i] = StationType.Enemy;
					array[1, i] = StationType.Adventure;
					break;
				case 3:
				case 4:
					array[0, i] = StationType.Adventure;
					array[1, i] = StationType.Enemy;
					break;
				case 5:
					array[0, i] = StationType.Enemy;
					array[1, i] = StationType.Enemy;
					break;
				}
			}
			List<int> list3 = new List<int>();
			list3.Add(1);
			list3.Add(2);
			list3.Add(3);
			list3.Add(4);
			list3.Add(5);
			List<int> list4 = list3;
			list4.Shuffle(rng);
			for (int j = 0; j < 4; j++)
			{
				switch (list4[j])
				{
				case 1:
				case 2:
					array[2, j] = StationType.Gap;
					array[3, j] = StationType.EliteEnemy;
					break;
				case 3:
					array[2, j] = StationType.Shop;
					array[3, j] = StationType.EliteEnemy;
					break;
				case 4:
					array[2, j] = StationType.Gap;
					array[3, j] = StationType.Enemy;
					break;
				case 5:
					array[2, j] = StationType.Shop;
					array[3, j] = StationType.Enemy;
					break;
				}
			}
			return array;
		}

		// Token: 0x060001F5 RID: 501 RVA: 0x00005AF8 File Offset: 0x00003CF8
		private static StationType[,] GetAct3(RandomGen rng)
		{
			StationType[,] array = new StationType[4, 4];
			List<int> list = new List<int>();
			list.Add(1);
			list.Add(2);
			list.Add(3);
			list.Add(4);
			list.Add(5);
			List<int> list2 = list;
			list2.Shuffle(rng);
			for (int i = 0; i < 4; i++)
			{
				switch (list2[i])
				{
				case 1:
				case 2:
					array[0, i] = StationType.Enemy;
					array[1, i] = StationType.Adventure;
					break;
				case 3:
				case 4:
					array[0, i] = StationType.Adventure;
					array[1, i] = StationType.Enemy;
					break;
				case 5:
					array[0, i] = StationType.Enemy;
					array[1, i] = StationType.Enemy;
					break;
				}
			}
			List<int> list3 = new List<int>();
			list3.Add(1);
			list3.Add(2);
			list3.Add(3);
			list3.Add(4);
			list3.Add(5);
			List<int> list4 = list3;
			list4.Shuffle(rng);
			for (int j = 0; j < 4; j++)
			{
				switch (list4[j])
				{
				case 1:
				case 2:
					array[2, j] = StationType.Gap;
					array[3, j] = StationType.EliteEnemy;
					break;
				case 3:
				case 4:
					array[2, j] = StationType.Shop;
					array[3, j] = StationType.EliteEnemy;
					break;
				case 5:
					array[2, j] = StationType.Shop;
					array[3, j] = StationType.Enemy;
					break;
				}
			}
			return array;
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x00005C58 File Offset: 0x00003E58
		private static void CreateSideWays(GameMap map, RandomGen rng, int xFloor, int xCeil)
		{
			List<int> list = new List<int>();
			list.Add(1);
			list.Add(2);
			list.Add(3);
			List<int> list2 = list;
			list2.Shuffle(rng);
			if (rng.NextFloat(0f, 1f) < 0.75f)
			{
				list2.Remove(list2[0]);
			}
			foreach (int num in list2)
			{
				int num2 = rng.NextInt(xFloor, xCeil - 2);
				if (rng.NextFloat(0f, 1f) < 0.5f)
				{
					map.Nodes[num2, num].FollowerList.Add(num + 1);
				}
				else
				{
					map.Nodes[num2, num + 1].FollowerList.Add(num);
				}
			}
		}

		// Token: 0x0400010C RID: 268
		private readonly List<MapNode> _path = new List<MapNode>();

		// Token: 0x020001CF RID: 463
		public enum UiType
		{
			// Token: 0x04000736 RID: 1846
			SingleRoute,
			// Token: 0x04000737 RID: 1847
			ThreeRoute,
			// Token: 0x04000738 RID: 1848
			NormalGame
		}
	}
}
