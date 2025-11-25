using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class QinaKapian : ShiningExhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UsUsingEventArgs>(base.Battle.UsUsed, new EventSequencedReactor<UsUsingEventArgs>(this.OnUsUsed));
		}
		private IEnumerable<BattleAction> OnUsUsed(UsUsingEventArgs args)
		{
			base.NotifyActivating();
			yield return new DamageAction(base.Owner, base.Owner, DamageInfo.HpLose((float)base.Value1, false), "Instant", GunType.Single);
			yield return new DrawManyCardAction(base.Value2);
			yield break;
		}
	}
}
