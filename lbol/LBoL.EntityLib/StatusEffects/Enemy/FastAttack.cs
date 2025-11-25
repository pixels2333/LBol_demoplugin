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
	[UsedImplicitly]
	public sealed class FastAttack : StatusEffect
	{
		protected override string GetBaseDescription()
		{
			if (!this.Active)
			{
				return base.ExtraDescription;
			}
			return base.GetBaseDescription();
		}
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
		private bool _active;
		private List<int> _chatIndicator;
	}
}
