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
	// Token: 0x0200007D RID: 125
	public class EnemyGroupEntry : IEnumerable<EnemyGroupEntry.EntrySource>, IEnumerable
	{
		// Token: 0x170001C8 RID: 456
		// (get) Token: 0x060005BB RID: 1467 RVA: 0x000127FC File Offset: 0x000109FC
		public EnemyGroupConfig Config { get; }

		// Token: 0x170001C9 RID: 457
		// (get) Token: 0x060005BC RID: 1468 RVA: 0x00012804 File Offset: 0x00010A04
		public string Id
		{
			get
			{
				return this.Config.Id;
			}
		}

		// Token: 0x170001CA RID: 458
		// (get) Token: 0x060005BD RID: 1469 RVA: 0x00012811 File Offset: 0x00010A11
		public EnemyType EnemyType
		{
			get
			{
				return this.Config.EnemyType;
			}
		}

		// Token: 0x170001CB RID: 459
		// (get) Token: 0x060005BE RID: 1470 RVA: 0x0001281E File Offset: 0x00010A1E
		public bool RollBossExhibit
		{
			get
			{
				return this.Config.RollBossExhibit;
			}
		}

		// Token: 0x170001CC RID: 460
		// (get) Token: 0x060005BF RID: 1471 RVA: 0x0001282B File Offset: 0x00010A2B
		public string FormationName
		{
			get
			{
				return this.Config.FormationName;
			}
		}

		// Token: 0x170001CD RID: 461
		// (get) Token: 0x060005C0 RID: 1472 RVA: 0x00012838 File Offset: 0x00010A38
		public Vector2 PlayerRootV2
		{
			get
			{
				return this.Config.PlayerRoot;
			}
		}

		// Token: 0x170001CE RID: 462
		// (get) Token: 0x060005C1 RID: 1473 RVA: 0x00012845 File Offset: 0x00010A45
		public string PreBattleDialogName
		{
			get
			{
				return this.Config.PreBattleDialogName;
			}
		}

		// Token: 0x170001CF RID: 463
		// (get) Token: 0x060005C2 RID: 1474 RVA: 0x00012852 File Offset: 0x00010A52
		public string PostBattleDialogName
		{
			get
			{
				return this.Config.PostBattleDialogName;
			}
		}

		// Token: 0x170001D0 RID: 464
		// (get) Token: 0x060005C3 RID: 1475 RVA: 0x0001285F File Offset: 0x00010A5F
		public bool Hidden
		{
			get
			{
				return this.Config.Hidden;
			}
		}

		// Token: 0x170001D1 RID: 465
		// (get) Token: 0x060005C4 RID: 1476 RVA: 0x0001286C File Offset: 0x00010A6C
		public float DebutTime
		{
			get
			{
				return this.Config.DebutTime;
			}
		}

		// Token: 0x170001D2 RID: 466
		// (get) Token: 0x060005C5 RID: 1477 RVA: 0x00012879 File Offset: 0x00010A79
		public string Environment
		{
			get
			{
				return this.Config.Environment;
			}
		}

		// Token: 0x060005C6 RID: 1478 RVA: 0x00012886 File Offset: 0x00010A86
		public EnemyGroupEntry(EnemyGroupConfig config)
		{
			this.Config = config;
		}

		// Token: 0x060005C7 RID: 1479 RVA: 0x000128A0 File Offset: 0x00010AA0
		public void Add(Type type)
		{
			this._entries.Add(new EnemyGroupEntry.EntrySource(type, this._entries.Count));
		}

		// Token: 0x060005C8 RID: 1480 RVA: 0x000128BE File Offset: 0x00010ABE
		public void Add(Type type, int rootIndex)
		{
			this._entries.Add(new EnemyGroupEntry.EntrySource(type, rootIndex));
		}

		// Token: 0x060005C9 RID: 1481 RVA: 0x000128D4 File Offset: 0x00010AD4
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

		// Token: 0x060005CA RID: 1482 RVA: 0x000129A8 File Offset: 0x00010BA8
		public IEnumerator<EnemyGroupEntry.EntrySource> GetEnumerator()
		{
			return this._entries.GetEnumerator();
		}

		// Token: 0x060005CB RID: 1483 RVA: 0x000129BA File Offset: 0x00010BBA
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x060005CC RID: 1484 RVA: 0x000129C4 File Offset: 0x00010BC4
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

		// Token: 0x040002D5 RID: 725
		private readonly List<EnemyGroupEntry.EntrySource> _entries = new List<EnemyGroupEntry.EntrySource>();

		// Token: 0x040002D6 RID: 726
		private List<EnemyGroupEntry.EntrySource> _list;

		// Token: 0x02000225 RID: 549
		public class EntrySource
		{
			// Token: 0x17000590 RID: 1424
			// (get) Token: 0x06001176 RID: 4470 RVA: 0x0002F4D3 File Offset: 0x0002D6D3
			public Type Type { get; }

			// Token: 0x17000591 RID: 1425
			// (get) Token: 0x06001177 RID: 4471 RVA: 0x0002F4DB File Offset: 0x0002D6DB
			public int RootIndex { get; }

			// Token: 0x06001178 RID: 4472 RVA: 0x0002F4E3 File Offset: 0x0002D6E3
			public EntrySource(Type type, int rootIndex)
			{
				this.Type = type;
				this.RootIndex = rootIndex;
			}
		}
	}
}
