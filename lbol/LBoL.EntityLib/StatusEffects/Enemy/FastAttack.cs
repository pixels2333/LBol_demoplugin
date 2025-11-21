using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000A2 RID: 162
	[UsedImplicitly]
	public sealed class FastAttack : StatusEffect
	{
		// Token: 0x06000241 RID: 577 RVA: 0x00006A4D File Offset: 0x00004C4D
		protected override string GetBaseDescription()
		{
			if (!this.Active)
			{
				return base.ExtraDescription;
			}
			return base.GetBaseDescription();
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000242 RID: 578 RVA: 0x00006A64 File Offset: 0x00004C64
		// (set) Token: 0x06000243 RID: 579 RVA: 0x00006A6C File Offset: 0x00004C6C
		private bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				this._active = value;
				base.Highlight = value;
			}
		}

		// Token: 0x06000244 RID: 580 RVA: 0x00006A7C File Offset: 0x00004C7C
		protected override void OnAdded(Unit unit)
		{
			if (unit is PlayerUnit)
			{
				Debug.LogError(this.DebugName + " should not apply to player unit.");
			}
			base.HandleOwnerEvent<UnitEventArgs>(base.Owner.TurnEnded, delegate(UnitEventArgs _)
			{
				this.Active = true;
			});
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			if (Random.Range(0f, 1f) > 0.5f)
			{
				List<int> list = new List<int>();
				list.Add(1);
				list.Add(2);
				this._chatIndicator = list;
			}
			else
			{
				List<int> list2 = new List<int>();
				list2.Add(2);
				list2.Add(1);
				this._chatIndicator = list2;
			}
			this.Active = true;
		}

		// Token: 0x06000245 RID: 581 RVA: 0x00006B31 File Offset: 0x00004D31
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Battle.BattleShouldEnd || !base.Battle.Player.IsInTurn || !this.Active)
			{
				yield break;
			}
			this.Active = false;
			base.NotifyActivating();
			List<int> chatIndicator = this._chatIndicator;
			if (chatIndicator != null && chatIndicator.Count > 0)
			{
				int num = Enumerable.First<int>(this._chatIndicator);
				this._chatIndicator.Remove(num);
				yield return PerformAction.Chat(base.Owner, string.Format("Chat.YaTiangou{0}", num).Localize(true), 3f, 0.3f, 0f, true);
			}
			string text = ((base.Level >= 15) ? "鸦天狗触发B" : "鸦天狗触发");
			DamageAction damageaction = new DamageAction(base.Owner, base.Battle.Player, DamageInfo.Attack((float)base.Level, false), text, GunType.Single);
			yield return damageaction;
			yield return new StatisticalTotalDamageAction(new DamageAction[] { damageaction });
			yield break;
		}

		// Token: 0x04000019 RID: 25
		private bool _active;

		// Token: 0x0400001A RID: 26
		private List<int> _chatIndicator;
	}
}
