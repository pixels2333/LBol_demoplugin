using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	[UsedImplicitly]
	public sealed class YouxiangWakeSe : StatusEffect
	{
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Empty;
			}
		}
		protected override string GetBaseDescription()
		{
			if (base.Count != 1)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			int num = base.Count - 1;
			base.Count = num;
			if (base.Count == 1)
			{
				base.Highlight = true;
			}
			if (base.Count <= 0)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Firepower>(base.Battle.Player, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
				if (base.Battle.DrawZone.Count > 0)
				{
					int max = base.Limit;
					int i = 1;
					while (i <= max && base.Battle.DrawZone.Count > 0 && !base.Battle.HandIsFull)
					{
						List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.DrawZone, (Card card) => card.CardType == CardType.Attack));
						if (list.Count > 0)
						{
							Card card2 = list.Sample(base.GameRun.BattleRng);
							card2.SetTurnCost(this.Mana);
							yield return new MoveCardAction(card2, CardZone.Hand);
						}
						num = i;
						i = num + 1;
					}
				}
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
	}
}
