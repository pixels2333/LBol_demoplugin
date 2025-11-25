using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Dolls
{
	[UsedImplicitly]
	public sealed class ManaDoll : Doll
	{
		[UsedImplicitly]
		public ManaGroup Mana1
		{
			get
			{
				return ManaGroup.Philosophies(1);
			}
		}
		[UsedImplicitly]
		public ManaGroup Mana2
		{
			get
			{
				return ManaGroup.Philosophies(2);
			}
		}
		protected override IEnumerable<BattleAction> PassiveActions()
		{
			base.NotifyPassiveActivating();
			yield return new GainManaAction(this.Mana1);
			yield break;
		}
		protected override IEnumerable<BattleAction> ActiveActions()
		{
			base.NotifyActiveActivating();
			yield return new GainManaAction(this.Mana2);
			yield break;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			base.NotifyActiveActivating();
			yield return new GainManaAction(this.Mana2);
			yield break;
		}
	}
}
