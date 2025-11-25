using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using UnityEngine;
namespace LBoL.Core.Units
{
	public class EnemyGroup : IEnumerable<EnemyUnit>, IEnumerable
	{
		public string Id { get; }
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
		public EnemyType EnemyType { get; }
		public string FormationName { get; }
		public Vector2 PlayerRootV2 { get; }
		public string PreBattleDialogName { get; }
		public string PostBattleDialogName { get; }
		public bool Hidden { get; }
		public float DebutTime { get; }
		public string Environment { get; }
		public int Count
		{
			get
			{
				return this._enemies.Count;
			}
		}
		public IEnumerable<EnemyUnit> Alives
		{
			get
			{
				return Enumerable.Where<EnemyUnit>(this._enemies, (EnemyUnit e) => e.IsAlive);
			}
		}
		public IEnumerable<EnemyUnit> Deads
		{
			get
			{
				return Enumerable.Where<EnemyUnit>(this._enemies, (EnemyUnit e) => e.IsDead);
			}
		}
		public IEnumerator<EnemyUnit> GetEnumerator()
		{
			return this._enemies.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		internal void Add(EnemyUnit enemy)
		{
			int num = this._index + 1;
			this._index = num;
			enemy.Index = num;
			this._enemies.Add(enemy);
		}
		private readonly List<EnemyUnit> _enemies;
		private int _index;
	}
}
