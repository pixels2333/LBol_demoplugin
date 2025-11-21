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
	// Token: 0x020000A8 RID: 168
	public sealed class JunkoColor : StatusEffect
	{
		// Token: 0x17000038 RID: 56
		// (get) Token: 0x06000258 RID: 600 RVA: 0x00006D17 File Offset: 0x00004F17
		[UsedImplicitly]
		public UnitName JunkoName
		{
			get
			{
				return this._junko.GetName();
			}
		}

		// Token: 0x06000259 RID: 601 RVA: 0x00006D24 File Offset: 0x00004F24
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

		// Token: 0x0600025A RID: 602 RVA: 0x00006D5C File Offset: 0x00004F5C
		protected override void OnAdded(Unit unit)
		{
			this._junko = (Junko)Enumerable.FirstOrDefault<EnemyUnit>(base.Battle.AllAliveEnemies, (EnemyUnit enemy) => enemy is Junko);
			base.ReactOwnerEvent<ManaEventArgs>(base.Battle.ManaGained, new EventSequencedReactor<ManaEventArgs>(this.OnManaGained));
		}

		// Token: 0x0600025B RID: 603 RVA: 0x00006DC0 File Offset: 0x00004FC0
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

		// Token: 0x0600025C RID: 604 RVA: 0x00006DD8 File Offset: 0x00004FD8
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

		// Token: 0x0400001B RID: 27
		private Junko _junko;
	}
}
