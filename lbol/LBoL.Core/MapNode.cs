using System;
using System.Collections.Generic;
using LBoL.Core.Stations;
namespace LBoL.Core
{
	public class MapNode
	{
		public GameMap Map
		{
			get
			{
				GameMap gameMap;
				if (!this._map.TryGetTarget(ref gameMap))
				{
					return null;
				}
				return gameMap;
			}
		}
		internal MapNode(GameMap map, int x, int y, int act = 1)
		{
			this._map = new WeakReference<GameMap>(map);
			this.X = x;
			this.Y = y;
			this.Act = act;
		}
		public MapNodeStatus Status { get; internal set; }
		public StationType StationType { get; set; }
		public int X { get; }
		public int Y { get; }
		public int Act { get; }
		public List<int> FollowerList { get; } = new List<int>();
		private readonly WeakReference<GameMap> _map;
	}
}
