using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using UnityEngine;
namespace LBoL.Core.Units
{
	public class EnemyGroupEntry : IEnumerable<EnemyGroupEntry.EntrySource>, IEnumerable
	{
		public EnemyGroupConfig Config { get; }
		public string Id
		{
			get
			{
				return this.Config.Id;
			}
		}
		public EnemyType EnemyType
		{
			get
			{
				return this.Config.EnemyType;
			}
		}
		public bool RollBossExhibit
		{
			get
			{
				return this.Config.RollBossExhibit;
			}
		}
		public string FormationName
		{
			get
			{
				return this.Config.FormationName;
			}
		}
		public Vector2 PlayerRootV2
		{
			get
			{
				return this.Config.PlayerRoot;
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
		public bool Hidden
		{
			get
			{
				return this.Config.Hidden;
			}
		}
		public float DebutTime
		{
			get
			{
				return this.Config.DebutTime;
			}
		}
		public string Environment
		{
			get
			{
				return this.Config.Environment;
			}
		}
		public EnemyGroupEntry(EnemyGroupConfig config)
		{
			this.Config = config;
		}
		public void Add(Type type)
		{
			this._entries.Add(new EnemyGroupEntry.EntrySource(type, this._entries.Count));
		}
		public void Add(Type type, int rootIndex)
		{
			this._entries.Add(new EnemyGroupEntry.EntrySource(type, rootIndex));
		}
		public EnemyGroup Generate(GameRunController gameRun)
		{
			if (!this.Config.IsSub && Enumerable.Any<string>(this.Config.Subs))
			{
				EnemyGroupConfig enemyGroupConfig = EnemyGroupConfig.FromId(this.Config.Subs.Sample(gameRun.StationRng));
				if (enemyGroupConfig != null)
				{
					return Library.CreateEnemyGroupEntryFromConfig(enemyGroupConfig).Generate(gameRun);
				}
			}
			EnemyGroup enemyGroup = new EnemyGroup(this.Id, this._entries, this.EnemyType, this.FormationName, this.PlayerRootV2, this.PreBattleDialogName, this.PostBattleDialogName, this.Hidden, this.DebutTime, this.Environment);
			foreach (EnemyUnit enemyUnit in enemyGroup)
			{
				enemyUnit.EnterGameRun(gameRun);
			}
			return enemyGroup;
		}
		public IEnumerator<EnemyGroupEntry.EntrySource> GetEnumerator()
		{
			return this._entries.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		public List<EnemyGroupEntry.EntrySource> ToList()
		{
			if (this._list != null)
			{
				return this._list;
			}
			this._list = Enumerable.ToList<EnemyGroupEntry.EntrySource>(Enumerable.Repeat<EnemyGroupEntry.EntrySource>(null, 5));
			foreach (EnemyGroupEntry.EntrySource entrySource in this._entries)
			{
				if (this._list[entrySource.RootIndex] != null)
				{
					throw new InvalidOperationException("'" + this.Id + "' has duplicated root index");
				}
				this._list[entrySource.RootIndex] = entrySource;
			}
			return this._list;
		}
		private readonly List<EnemyGroupEntry.EntrySource> _entries = new List<EnemyGroupEntry.EntrySource>();
		private List<EnemyGroupEntry.EntrySource> _list;
		public class EntrySource
		{
			public Type Type { get; }
			public int RootIndex { get; }
			public EntrySource(Type type, int rootIndex)
			{
				this.Type = type;
				this.RootIndex = rootIndex;
			}
		}
	}
}
