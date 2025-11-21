using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x02000076 RID: 118
	public sealed class UnitSelector
	{
		// Token: 0x17000192 RID: 402
		// (get) Token: 0x06000536 RID: 1334 RVA: 0x000116D0 File Offset: 0x0000F8D0
		public TargetType Type { get; }

		// Token: 0x17000193 RID: 403
		// (get) Token: 0x06000537 RID: 1335 RVA: 0x000116D8 File Offset: 0x0000F8D8
		public EnemyUnit SelectedEnemy
		{
			get
			{
				if (this.Type != TargetType.SingleEnemy)
				{
					throw new InvalidOperationException(string.Format("Cannot get enemy with type '{0}'", this.Type));
				}
				return this._selectedEnemy;
			}
		}

		// Token: 0x06000538 RID: 1336 RVA: 0x00011704 File Offset: 0x0000F904
		private UnitSelector(TargetType type)
		{
			if (type == TargetType.SingleEnemy)
			{
				throw new ArgumentException(string.Format("Cannot create '{0}' with type {1}", "UnitSelector", type));
			}
			this.Type = type;
		}

		// Token: 0x06000539 RID: 1337 RVA: 0x00011732 File Offset: 0x0000F932
		public UnitSelector(EnemyUnit selectedEnemy)
		{
			this.Type = TargetType.SingleEnemy;
			this._selectedEnemy = selectedEnemy;
		}

		// Token: 0x0600053A RID: 1338 RVA: 0x00011748 File Offset: 0x0000F948
		[CanBeNull]
		public EnemyUnit GetEnemy(BattleController battle)
		{
			TargetType type = this.Type;
			EnemyUnit enemyUnit;
			if (type != TargetType.SingleEnemy)
			{
				if (type != TargetType.RandomEnemy)
				{
					throw new InvalidOperationException(string.Format("Cannot get single enemy target with type '{0}'", this.Type));
				}
				enemyUnit = battle.RandomAliveEnemy;
			}
			else
			{
				enemyUnit = this._selectedEnemy;
			}
			return enemyUnit;
		}

		// Token: 0x0600053B RID: 1339 RVA: 0x00011794 File Offset: 0x0000F994
		public EnemyUnit[] GetEnemies(BattleController battle)
		{
			if (this.Type == TargetType.AllEnemies)
			{
				return Enumerable.ToArray<EnemyUnit>(battle.EnemyGroup.Alives);
			}
			throw new InvalidOperationException(string.Format("Cannot get multi enemy target with type '{0}'", this.Type));
		}

		// Token: 0x0600053C RID: 1340 RVA: 0x000117DC File Offset: 0x0000F9DC
		public Unit[] GetUnits(BattleController battle)
		{
			Unit[] array;
			switch (this.Type)
			{
			case TargetType.Nobody:
				array = Array.Empty<Unit>();
				break;
			case TargetType.SingleEnemy:
				array = new Unit[] { this._selectedEnemy };
				break;
			case TargetType.AllEnemies:
				array = Enumerable.ToArray<Unit>(battle.EnemyGroup.Alives);
				break;
			case TargetType.RandomEnemy:
				array = new Unit[] { battle.RandomAliveEnemy };
				break;
			case TargetType.Self:
				array = new Unit[] { battle.Player };
				break;
			case TargetType.All:
				array = Enumerable.ToArray<Unit>(Enumerable.Concat<Unit>(new Unit[] { battle.Player }, battle.EnemyGroup.Alives));
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return array;
		}

		// Token: 0x0600053D RID: 1341 RVA: 0x00011890 File Offset: 0x0000FA90
		public override string ToString()
		{
			string text;
			switch (this.Type)
			{
			case TargetType.Nobody:
				text = "Selector: Nobody";
				break;
			case TargetType.SingleEnemy:
				text = "Selector: " + this._selectedEnemy.DebugName;
				break;
			case TargetType.AllEnemies:
				text = "Selector: AllEnemies";
				break;
			case TargetType.RandomEnemy:
				text = "Selector: RandomEnemy";
				break;
			case TargetType.Self:
				text = "Selector: Self";
				break;
			case TargetType.All:
				text = "Selector: All";
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return text;
		}

		// Token: 0x040002AD RID: 685
		private readonly EnemyUnit _selectedEnemy;

		// Token: 0x040002AE RID: 686
		public static readonly UnitSelector Nobody = new UnitSelector(TargetType.Nobody);

		// Token: 0x040002AF RID: 687
		public static readonly UnitSelector AllEnemies = new UnitSelector(TargetType.AllEnemies);

		// Token: 0x040002B0 RID: 688
		public static readonly UnitSelector RandomEnemy = new UnitSelector(TargetType.RandomEnemy);

		// Token: 0x040002B1 RID: 689
		public static readonly UnitSelector Self = new UnitSelector(TargetType.Self);

		// Token: 0x040002B2 RID: 690
		public static readonly UnitSelector All = new UnitSelector(TargetType.All);
	}
}
