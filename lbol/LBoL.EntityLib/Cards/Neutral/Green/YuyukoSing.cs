using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.Green
{
	[UsedImplicitly]
	public sealed class YuyukoSing : Card
	{
		public override bool CanUpgrade
		{
			get
			{
				int? upgradeCounter = base.UpgradeCounter;
				int num = 99;
				return (upgradeCounter.GetValueOrDefault() < num) & (upgradeCounter != null);
			}
		}
		public override bool IsUpgraded
		{
			get
			{
				return base.UpgradeCounter > 0;
			}
		}
		public override void Initialize()
		{
			base.Initialize();
			base.UpgradeCounter = new int?(0);
		}
		public override void Upgrade()
		{
			int? num = base.UpgradeCounter + 1;
			base.UpgradeCounter = num;
			base.ProcessKeywordUpgrade();
			base.CostChangeInUpgrading();
			this.NotifyChanged();
		}
		protected override int AdditionalDamage
		{
			get
			{
				if (!(base.UpgradeCounter > 0))
				{
					return 0;
				}
				return (base.UpgradeCounter.Value + 5) * base.UpgradeCounter.Value;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int num = ((base.UpgradeCounter > 0) ? base.UpgradeCounter.Value : 0);
			string text = "蝶之羽风" + Math.Min(num, 4).ToString();
			yield return base.AttackAction(selector, text);
			yield break;
		}
	}
}
