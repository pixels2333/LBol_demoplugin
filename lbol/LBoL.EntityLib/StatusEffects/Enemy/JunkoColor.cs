using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	public sealed class JunkoColor : StatusEffect
	{
		[UsedImplicitly]
		public UnitName JunkoName
		{
			get
			{
				return this._junko.GetName();
			}
		}
		protected override string GetBaseDescription()
		{
			int level = base.Level;
			string text;
			if (level < 3)
			{
				if (level != 2)
				{
					text = base.GetBaseDescription();
				}
				else
				{
					text = base.ExtraDescription;
				}
			}
			else
			{
				text = base.ExtraDescription2;
			}
			return text;
		}
		protected override void OnAdded(Unit unit)
		{
			this._junko = (Junko)Enumerable.FirstOrDefault<EnemyUnit>(base.Battle.AllAliveEnemies, (EnemyUnit enemy) => enemy is Junko);
			base.ReactOwnerEvent<ManaEventArgs>(base.Battle.ManaGained, new EventSequencedReactor<ManaEventArgs>(this.OnManaGained));
		}
		private IEnumerable<BattleAction> OnManaGained(ManaEventArgs args)
		{
			if (!base.Battle.BattleShouldEnd)
			{
				Junko junko = this._junko;
				if (junko != null && junko.IsAlive)
				{
					ManaGroup value = args.Value;
					if (value.Philosophy > 0)
					{
						base.Count += value.Philosophy;
						int level = JunkoColor.GetLevel(base.Count);
						if (level > base.Level)
						{
							base.Level = level;
							base.NotifyActivating();
							foreach (BattleAction battleAction in this._junko.JunkoColorActions(base.Level))
							{
								yield return battleAction;
							}
							IEnumerator<BattleAction> enumerator = null;
						}
					}
					yield break;
				}
			}
			yield break;
			yield break;
		}
		private static int GetLevel(int count)
		{
			int num;
			if (count < 40)
			{
				if (count > 0)
				{
					if (count >= 10)
					{
						num = 3;
					}
					else
					{
						num = 2;
					}
				}
				else
				{
					num = 1;
				}
			}
			else if (count >= 100)
			{
				num = 5;
			}
			else
			{
				num = 4;
			}
			return num;
		}
		private Junko _junko;
	}
}
