using System;
using System.Collections.Generic;
using LBoL.Core.Stations;

namespace LBoL.Core
{
	// Token: 0x0200005E RID: 94
	public class MapNode
	{
		// Token: 0x17000149 RID: 329
		// (get) Token: 0x0600041E RID: 1054 RVA: 0x0000E6F8 File Offset: 0x0000C8F8
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

		// Token: 0x0600041F RID: 1055 RVA: 0x0000E717 File Offset: 0x0000C917
		internal MapNode(GameMap map, int x, int y, int act = 1)
		{
			this._map = new WeakReference<GameMap>(map);
			this.X = x;
			this.Y = y;
			this.Act = act;
		}

		// Token: 0x1700014A RID: 330
		// (get) Token: 0x06000420 RID: 1056 RVA: 0x0000E74C File Offset: 0x0000C94C
		// (set) Token: 0x06000421 RID: 1057 RVA: 0x0000E754 File Offset: 0x0000C954
		public MapNodeStatus Status { get; internal set; }

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x06000422 RID: 1058 RVA: 0x0000E75D File Offset: 0x0000C95D
		// (set) Token: 0x06000423 RID: 1059 RVA: 0x0000E765 File Offset: 0x0000C965
		public StationType StationType { get; set; }

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x06000424 RID: 1060 RVA: 0x0000E76E File Offset: 0x0000C96E
		public int X { get; }

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x06000425 RID: 1061 RVA: 0x0000E776 File Offset: 0x0000C976
		public int Y { get; }

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x06000426 RID: 1062 RVA: 0x0000E77E File Offset: 0x0000C97E
		public int Act { get; }

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x06000427 RID: 1063 RVA: 0x0000E786 File Offset: 0x0000C986
		public List<int> FollowerList { get; } = new List<int>();

		// Token: 0x04000239 RID: 569
		private readonly WeakReference<GameMap> _map;
	}
}
