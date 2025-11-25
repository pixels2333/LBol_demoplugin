using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	[UsedImplicitly]
	public sealed class DreamServant : StatusEffect
	{
		private int TurnCounter { get; set; }
		protected override void OnAdded(Unit unit)
		{
			this.TurnCounter = 0;
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnDamageTaking));
			base.ReactOwnerEvent<UnitEventArgs>(unit.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnded));
		}
		private void OnDamageTaking(DamageEventArgs args)
		{
			args.DamageInfo = args.DamageInfo.MultiplyBy(0);
		}
		private IEnumerable<BattleAction> OnTurnEnded(UnitEventArgs args)
		{
			int num = this.TurnCounter + 1;
			this.TurnCounter = num;
			if (this.TurnCounter >= 2)
			{
				yield return new EscapeAction(base.Owner);
			}
			yield break;
		}
		public override string UnitEffectName
		{
			get
			{
				return "DreamLoop";
			}
		}
	}
}
