using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class ReimuHeavyAttack : Card
	{
		private int PlayerTotalFire
		{
			get
			{
				if (base.Battle == null)
				{
					return 0;
				}
				return base.Battle.Player.TotalFirepower;
			}
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<DamageDealingEventArgs>(base.Battle.Player.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnPlayerDamageDealing), (GameEventPriority)0);
		}
		private void OnPlayerDamageDealing(DamageDealingEventArgs args)
		{
			if (args.ActionSource == this && args.DamageInfo.DamageType == DamageType.Attack && this.PlayerTotalFire > 0)
			{
				args.DamageInfo = args.DamageInfo.IncreaseBy(this.PlayerTotalFire * (base.Value1 - 1));
				args.AddModifier(this);
			}
		}
	}
}
