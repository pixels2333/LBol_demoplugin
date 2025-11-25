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
	public class BattleSet : GameEntity, IEnumerable<EnemyUnit>, IEnumerable
	{
		public EnemyGroupConfig Config { get; private set; }
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return GameEventPriority.ConfigDefault;
			}
		}
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<BattleSet>.LocalizeProperty(base.Id, key, decorated, required);
		}
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
		public EnemyType EnemyType
		{
			get
			{
				return this.Config.EnemyType;
			}
		}
		public string FormationName
		{
			get
			{
				return this.Config.FormationName;
			}
		}
		public string PreBattleDialogName
		{
			get
			{
				return this.Config.PreBattleDialogName;
			}
		}
		public string PostBattleDialogName
		{
			get
			{
				return this.Config.PostBattleDialogName;
			}
		}
		public float DebutTime
		{
			get
			{
				return this.Config.DebutTime;
			}
		}
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
		private readonly List<EnemyUnit> _enemies = new List<EnemyUnit>();
		private int _index;
		public readonly Vector2 PlayerRootV2 = new Vector2(-4f, 0.5f);
	}
}
