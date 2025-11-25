using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Basic;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class DanmuDuijue : Card
	{
		protected override int AdditionalBlock
		{
			get
			{
				if (base.Battle != null && !this.IsUpgraded)
				{
					return this.PlayerFirepowerPositive;
				}
				return 0;
			}
		}
		protected override int AdditionalShield
		{
			get
			{
				if (base.Battle != null && this.IsUpgraded)
				{
					return this.PlayerFirepowerPositive;
				}
				return 0;
			}
		}
		protected override int AdditionalValue1
		{
			get
			{
				if (base.Battle != null)
				{
					return this.PlayerFirepowerPositive;
				}
				return 0;
			}
		}
		private int PlayerFirepowerPositive
		{
			get
			{
				return Math.Max(0, base.Battle.Player.TotalFirepower);
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<Reflect>(base.Value1, 0, 0, 0, 0.2f);
			if (base.Battle.Player.HasStatusEffect<Reflect>())
			{
				base.Battle.Player.GetStatusEffect<Reflect>().Gun = (this.IsUpgraded ? "弹幕对决B" : "弹幕对决");
			}
			yield break;
		}
	}
}
