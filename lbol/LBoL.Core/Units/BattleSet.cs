using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;

namespace LBoL.Core.Units
{
	// Token: 0x0200007A RID: 122
	public class BattleSet : GameEntity, IEnumerable<EnemyUnit>, IEnumerable
	{
		// Token: 0x1700019D RID: 413
		// (get) Token: 0x0600055C RID: 1372 RVA: 0x00011E48 File Offset: 0x00010048
		// (set) Token: 0x0600055D RID: 1373 RVA: 0x00011E50 File Offset: 0x00010050
		public EnemyGroupConfig Config { get; private set; }

		// Token: 0x1700019E RID: 414
		// (get) Token: 0x0600055E RID: 1374 RVA: 0x00011E59 File Offset: 0x00010059
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return GameEventPriority.ConfigDefault;
			}
		}

		// Token: 0x0600055F RID: 1375 RVA: 0x00011E5D File Offset: 0x0001005D
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<BattleSet>.LocalizeProperty(base.Id, key, decorated, required);
		}

		// Token: 0x06000560 RID: 1376 RVA: 0x00011E6D File Offset: 0x0001006D
		public override void Initialize()
		{
			base.Initialize();
			this.Config = EnemyGroupConfig.FromId(base.Id);
			if (this.Config == null)
			{
				throw new InvalidDataException("Cannot find battle-set config for " + base.Id);
			}
			this.GenerateEnemies();
		}

		// Token: 0x06000561 RID: 1377 RVA: 0x00011EAC File Offset: 0x000100AC
		protected virtual void GenerateEnemies()
		{
			foreach (ValueTuple<int, string> valueTuple in this.Config.Enemies.WithIndices<string>())
			{
				int item = valueTuple.Item1;
				string item2 = valueTuple.Item2;
				if (item2 != "Empty")
				{
					EnemyUnit enemyUnit = Library.CreateEnemyUnit(item2);
					enemyUnit.RootIndex = item;
					this._enemies.Add(enemyUnit);
				}
			}
		}

		// Token: 0x1700019F RID: 415
		// (get) Token: 0x06000562 RID: 1378 RVA: 0x00011F30 File Offset: 0x00010130
		public EnemyType EnemyType
		{
			get
			{
				return this.Config.EnemyType;
			}
		}

		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x06000563 RID: 1379 RVA: 0x00011F3D File Offset: 0x0001013D
		public string FormationName
		{
			get
			{
				return this.Config.FormationName;
			}
		}

		// Token: 0x170001A1 RID: 417
		// (get) Token: 0x06000564 RID: 1380 RVA: 0x00011F4A File Offset: 0x0001014A
		public string PreBattleDialogName
		{
			get
			{
				return this.Config.PreBattleDialogName;
			}
		}

		// Token: 0x170001A2 RID: 418
		// (get) Token: 0x06000565 RID: 1381 RVA: 0x00011F57 File Offset: 0x00010157
		public string PostBattleDialogName
		{
			get
			{
				return this.Config.PostBattleDialogName;
			}
		}

		// Token: 0x170001A3 RID: 419
		// (get) Token: 0x06000566 RID: 1382 RVA: 0x00011F64 File Offset: 0x00010164
		public float DebutTime
		{
			get
			{
				return this.Config.DebutTime;
			}
		}

		// Token: 0x170001A4 RID: 420
		// (get) Token: 0x06000567 RID: 1383 RVA: 0x00011F71 File Offset: 0x00010171
		public int Count
		{
			get
			{
				return this._enemies.Count;
			}
		}

		// Token: 0x170001A5 RID: 421
		// (get) Token: 0x06000568 RID: 1384 RVA: 0x00011F7E File Offset: 0x0001017E
		public IEnumerable<EnemyUnit> Alives
		{
			get
			{
				return Enumerable.Where<EnemyUnit>(this._enemies, (EnemyUnit e) => e.IsAlive);
			}
		}

		// Token: 0x06000569 RID: 1385 RVA: 0x00011FAA File Offset: 0x000101AA
		public IEnumerator<EnemyUnit> GetEnumerator()
		{
			return this._enemies.GetEnumerator();
		}

		// Token: 0x0600056A RID: 1386 RVA: 0x00011FBC File Offset: 0x000101BC
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x0600056B RID: 1387 RVA: 0x00011FC4 File Offset: 0x000101C4
		internal void Add(EnemyUnit enemy)
		{
			int num = this._index + 1;
			this._index = num;
			enemy.Index = num;
			this._enemies.Add(enemy);
		}

		// Token: 0x040002BD RID: 701
		private readonly List<EnemyUnit> _enemies = new List<EnemyUnit>();

		// Token: 0x040002BE RID: 702
		private int _index;

		// Token: 0x040002BF RID: 703
		public readonly Vector2 PlayerRootV2 = new Vector2(-4f, 0.5f);
	}
}
