using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000CA RID: 202
	[UsedImplicitly]
	public sealed class SuwakoHex : StatusEffect
	{
		// Token: 0x060002BA RID: 698 RVA: 0x00007742 File Offset: 0x00005942
		protected override string GetBaseDescription()
		{
			if (!this._active)
			{
				return base.ExtraDescription;
			}
			return base.GetBaseDescription();
		}

		// Token: 0x060002BB RID: 699 RVA: 0x0000775C File Offset: 0x0000595C
		protected override void OnAdded(Unit unit)
		{
			this._active = true;
			base.HandleOwnerEvent<UnitEventArgs>(base.Owner.TurnEnded, delegate(UnitEventArgs _)
			{
				this._active = true;
			});
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x060002BC RID: 700 RVA: 0x000077AF File Offset: 0x000059AF
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd || !this._active)
			{
				yield break;
			}
			List<Card> cards = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => !(card is Frog)));
			if (cards.Count > 0)
			{
				this._active = false;
				yield return PerformAction.Wait(0.2f, true);
				base.NotifyActivating();
				yield return PerformAction.UiSound("Frog");
				Card[] array = cards.SampleManyOrAll(base.Level, base.GameRun.EnemyBattleRng);
				foreach (Card card2 in array)
				{
					Frog frog = Library.CreateCard<Frog>();
					frog.OriginalCard = card2;
					yield return new TransformCardAction(card2, frog);
				}
				Card[] array2 = null;
			}
			yield break;
		}

		// Token: 0x0400001F RID: 31
		private bool _active;
	}
}
