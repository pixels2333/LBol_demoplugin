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
	public class GameMap
	{
		private GameMap(int maxLevels, int maxLevelNotes, GameMap.UiType type, [CanBeNull] string bossId)
		{
			this.Type = type;
			this.BossId = bossId;
			this.Nodes = new MapNode[maxLevels, maxLevelNotes];
		}
		public MapNode[,] Nodes { get; }
		public IEnumerable<MapNode> AllNodes
		{
			get
			{
				return Enumerable.Where<MapNode>(Enumerable.Cast<MapNode>(this.Nodes), (MapNode node) => node != null);
			}
		}
		public MapNode StartNode { get; private set; }
		public MapNode EndNode { get; private set; }
		public MapNode BossNode { get; private set; }
		public GameMap.UiType Type { get; }
		public string BossId { get; }
		public int Levels
		{
			get
			{
				return this.Nodes.GetLength(0);
			}
		}
		public int Width
		{
			get
			{
				return this.Nodes.GetLength(1);
			}
		}
		public MapNode VisitingNode { get; private set; }
		public IReadOnlyList<MapNode> Path
		{
			get
			{
				return this._path.AsReadOnly();
			}
		}
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
		public void RestorePath(IEnumerable<MapNode> nodes)
		{
			foreach (MapNode mapNode in nodes)
			{
				mapNode.Status = MapNodeStatus.Active;
				this.EnterNode(mapNode, true, true);
			}
		}
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
		private readonly List<MapNode> _path = new List<MapNode>();
		public enum UiType
		{
			SingleRoute,
			ThreeRoute,
			NormalGame
		}
	}
}
