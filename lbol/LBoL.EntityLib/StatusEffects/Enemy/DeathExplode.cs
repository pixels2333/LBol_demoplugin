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
	public abstract class DeathExplode : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			if (!(unit is EnemyUnit))
			{
				Debug.LogError("Cannot add DeathExplode to " + unit.GetType().Name);
			}
			base.ReactOwnerEvent<DieEventArgs>(base.Owner.Dying, new EventSequencedReactor<DieEventArgs>(this.OnDying));
		}
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
		public const int Multiplier = 2;
	}
}
