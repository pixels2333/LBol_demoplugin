using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using UnityEngine;

namespace LBoL.Core.Units
{
	// Token: 0x0200007C RID: 124
	public class EnemyGroup : IEnumerable<EnemyUnit>, IEnumerable
	{
		// Token: 0x170001BC RID: 444
		// (get) Token: 0x060005AB RID: 1451 RVA: 0x0001262E File Offset: 0x0001082E
		public string Id { get; }

		// Token: 0x060005AC RID: 1452 RVA: 0x00012638 File Offset: 0x00010838
		internal EnemyGroup(string id, IEnumerable<EnemyGroupEntry.EntrySource> entries, EnemyType enemyType, string formationName, Vector2 playerRootV2, string preBattleDialogName, string postBattleDialogName, bool hidden, float debutTime, string environment)
		{
			this.Id = id;
			this._enemies = new List<EnemyUnit>();
			foreach (EnemyGroupEntry.EntrySource entrySource in entries)
			{
				EnemyUnit enemyUnit = TypeFactory<EnemyUnit>.CreateInstance(entrySource.Type);
				EnemyUnit enemyUnit2 = enemyUnit;
				int num = this._index + 1;
				this._index = num;
				enemyUnit2.Index = num;
				enemyUnit.RootIndex = entrySource.RootIndex;
				this._enemies.Add(enemyUnit);
			}
			this.EnemyType = enemyType;
			this.FormationName = formationName;
			this.PlayerRootV2 = playerRootV2;
			this.PreBattleDialogName = preBattleDialogName;
			this.PostBattleDialogName = postBattleDialogName;
			this.Hidden = hidden;
			this.DebutTime = debutTime;
			this.Environment = environment;
		}

		// Token: 0x170001BD RID: 445
		// (get) Token: 0x060005AD RID: 1453 RVA: 0x0001270C File Offset: 0x0001090C
		public EnemyType EnemyType { get; }

		// Token: 0x170001BE RID: 446
		// (get) Token: 0x060005AE RID: 1454 RVA: 0x00012714 File Offset: 0x00010914
		public string FormationName { get; }

		// Token: 0x170001BF RID: 447
		// (get) Token: 0x060005AF RID: 1455 RVA: 0x0001271C File Offset: 0x0001091C
		public Vector2 PlayerRootV2 { get; }

		// Token: 0x170001C0 RID: 448
		// (get) Token: 0x060005B0 RID: 1456 RVA: 0x00012724 File Offset: 0x00010924
		public string PreBattleDialogName { get; }

		// Token: 0x170001C1 RID: 449
		// (get) Token: 0x060005B1 RID: 1457 RVA: 0x0001272C File Offset: 0x0001092C
		public string PostBattleDialogName { get; }

		// Token: 0x170001C2 RID: 450
		// (get) Token: 0x060005B2 RID: 1458 RVA: 0x00012734 File Offset: 0x00010934
		public bool Hidden { get; }

		// Token: 0x170001C3 RID: 451
		// (get) Token: 0x060005B3 RID: 1459 RVA: 0x0001273C File Offset: 0x0001093C
		public float DebutTime { get; }

		// Token: 0x170001C4 RID: 452
		// (get) Token: 0x060005B4 RID: 1460 RVA: 0x00012744 File Offset: 0x00010944
		public string Environment { get; }

		// Token: 0x170001C5 RID: 453
		// (get) Token: 0x060005B5 RID: 1461 RVA: 0x0001274C File Offset: 0x0001094C
		public int Count
		{
			get
			{
				return this._enemies.Count;
			}
		}

		// Token: 0x170001C6 RID: 454
		// (get) Token: 0x060005B6 RID: 1462 RVA: 0x00012759 File Offset: 0x00010959
		public IEnumerable<EnemyUnit> Alives
		{
			get
			{
				return Enumerable.Where<EnemyUnit>(this._enemies, (EnemyUnit e) => e.IsAlive);
			}
		}

		// Token: 0x170001C7 RID: 455
		// (get) Token: 0x060005B7 RID: 1463 RVA: 0x00012785 File Offset: 0x00010985
		public IEnumerable<EnemyUnit> Deads
		{
			get
			{
				return Enumerable.Where<EnemyUnit>(this._enemies, (EnemyUnit e) => e.IsDead);
			}
		}

		// Token: 0x060005B8 RID: 1464 RVA: 0x000127B1 File Offset: 0x000109B1
		public IEnumerator<EnemyUnit> GetEnumerator()
		{
			return this._enemies.GetEnumerator();
		}

		// Token: 0x060005B9 RID: 1465 RVA: 0x000127C3 File Offset: 0x000109C3
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x060005BA RID: 1466 RVA: 0x000127CC File Offset: 0x000109CC
		internal void Add(EnemyUnit enemy)
		{
			int num = this._index + 1;
			this._index = num;
			enemy.Index = num;
			this._enemies.Add(enemy);
		}

		// Token: 0x040002CA RID: 714
		private readonly List<EnemyUnit> _enemies;

		// Token: 0x040002CB RID: 715
		private int _index;
	}
}
