using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class YaoguaiBuster : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (this.IsUpgraded)
			{
				base.CardGuns = new Guns(new string[]
				{
					base.Config.GunNameBurst,
					base.Config.GunName,
					base.Config.GunNameBurst
				});
			}
			else
			{
				base.CardGuns = new Guns(new string[]
				{
					base.Config.GunName,
					base.Config.GunNameBurst
				});
			}
			foreach (GunPair gunPair in base.CardGuns.GunPairs)
			{
				yield return base.AttackAction(selector, gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			EnemyUnit selectedEnemy = selector.SelectedEnemy;
			if (selectedEnemy.IsAlive)
			{
				yield return base.DebuffAction<Weak>(selectedEnemy, 0, base.Value2, 0, 0, true, 0.2f);
			}
			yield break;
			yield break;
		}
	}
}
