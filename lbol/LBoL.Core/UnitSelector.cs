using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public sealed class UnitSelector
	{
		public TargetType Type { get; }
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
		private UnitSelector(TargetType type)
		{
			if (type == TargetType.SingleEnemy)
			{
				throw new ArgumentException(string.Format("Cannot create '{0}' with type {1}", "UnitSelector", type));
			}
			this.Type = type;
		}
		public UnitSelector(EnemyUnit selectedEnemy)
		{
			this.Type = TargetType.SingleEnemy;
			this._selectedEnemy = selectedEnemy;
		}
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
		public EnemyUnit[] GetEnemies(BattleController battle)
		{
			if (this.Type == TargetType.AllEnemies)
			{
				return Enumerable.ToArray<EnemyUnit>(battle.EnemyGroup.Alives);
			}
			throw new InvalidOperationException(string.Format("Cannot get multi enemy target with type '{0}'", this.Type));
		}
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
		private readonly EnemyUnit _selectedEnemy;
		public static readonly UnitSelector Nobody = new UnitSelector(TargetType.Nobody);
		public static readonly UnitSelector AllEnemies = new UnitSelector(TargetType.AllEnemies);
		public static readonly UnitSelector RandomEnemy = new UnitSelector(TargetType.RandomEnemy);
		public static readonly UnitSelector Self = new UnitSelector(TargetType.Self);
		public static readonly UnitSelector All = new UnitSelector(TargetType.All);
	}
}
