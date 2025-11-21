using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Reimu;

namespace LBoL.EntityLib.StatusEffects.Reimu
{
	// Token: 0x02000027 RID: 39
	[UsedImplicitly]
	public sealed class BoliPhantomSe : StatusEffect
	{
		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000066 RID: 102 RVA: 0x00002B2F File Offset: 0x00000D2F
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Empty;
			}
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00002B36 File Offset: 0x00000D36
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardEventArgs>(base.Battle.CardExiled, new EventSequencedReactor<CardEventArgs>(this.OnCardExiled));
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00002B55 File Offset: 0x00000D55
		private IEnumerable<BattleAction> OnCardExiled(CardEventArgs args)
		{
			if (args.Cause != ActionCause.AutoExile && !(args.Card is Fengmozhen))
			{
				List<Card> list = new List<Card>();
				for (int i = 0; i < base.Level; i++)
				{
					Fengmozhen fengmozhen = Library.CreateCard<Fengmozhen>();
					fengmozhen.IsPhantom = true;
					fengmozhen.SetTurnCost(this.Mana);
					list.Add(fengmozhen);
				}
				base.NotifyActivating();
				yield return new AddCardsToHandAction(list, AddCardsType.Normal);
			}
			yield break;
		}
	}
}
