using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal.Guihuos;
using UnityEngine;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x02000091 RID: 145
	public abstract class DeathExplode : StatusEffect
	{
		// Token: 0x06000211 RID: 529 RVA: 0x000064EC File Offset: 0x000046EC
		protected override void OnAdded(Unit unit)
		{
			if (!(unit is EnemyUnit))
			{
				Debug.LogError("Cannot add DeathExplode to " + unit.GetType().Name);
			}
			base.ReactOwnerEvent<DieEventArgs>(base.Owner.Dying, new EventSequencedReactor<DieEventArgs>(this.OnDying));
		}

		// Token: 0x06000212 RID: 530 RVA: 0x00006538 File Offset: 0x00004738
		private IEnumerable<BattleAction> OnDying(DieEventArgs args)
		{
			DieCause dieCause = args.DieCause;
			if (dieCause == DieCause.Explode || dieCause == DieCause.ServantDie)
			{
				yield break;
			}
			base.NotifyActivating();
			args.CancelBy(this);
			string text = "GuihuoExplode";
			Unit owner = base.Owner;
			if (!(owner is GuihuoRed))
			{
				if (!(owner is GuihuoGreen))
				{
					if (owner is GuihuoBlue)
					{
						text += "B";
					}
				}
				else
				{
					text += "G";
				}
			}
			else
			{
				text += "R";
			}
			int num = base.Level;
			if (args.DieCause == DieCause.ForceKill && args.Source != base.Battle.Player)
			{
				text += "2";
				num *= 2;
			}
			else
			{
				text += "1";
			}
			yield return new ExplodeAction((EnemyUnit)base.Owner, base.Battle.Player, DamageInfo.Attack((float)num, false), DieCause.Explode, args.DieSource, text, GunType.Single);
			yield break;
		}

		// Token: 0x04000014 RID: 20
		public const int Multiplier = 2;
	}
}
