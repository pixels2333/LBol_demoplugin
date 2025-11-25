using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.NoColor
{
	[UsedImplicitly]
	public sealed class Shoot : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			string text2;
			if (this.IsUpgraded)
			{
				string text;
				switch (consumingMana.MaxColor)
				{
				case ManaColor.White:
					text = "ShootW1";
					goto IL_0093;
				case ManaColor.Blue:
					text = "ShootU1";
					goto IL_0093;
				case ManaColor.Black:
					text = "ShootB1";
					goto IL_0093;
				case ManaColor.Red:
					text = "ShootR1";
					goto IL_0093;
				case ManaColor.Green:
					text = "ShootG1";
					goto IL_0093;
				case ManaColor.Philosophy:
					text = "ShootP1";
					goto IL_0093;
				}
				text = "ShootC1";
				IL_0093:
				text2 = text;
			}
			else
			{
				string text;
				switch (consumingMana.MaxColor)
				{
				case ManaColor.White:
					text = "ShootW";
					goto IL_0101;
				case ManaColor.Blue:
					text = "ShootU";
					goto IL_0101;
				case ManaColor.Black:
					text = "ShootB";
					goto IL_0101;
				case ManaColor.Red:
					text = "ShootR";
					goto IL_0101;
				case ManaColor.Green:
					text = "ShootG";
					goto IL_0101;
				case ManaColor.Philosophy:
					text = "ShootP";
					goto IL_0101;
				}
				text = "ShootC";
				IL_0101:
				text2 = text;
			}
			yield return base.AttackAction(selector, text2);
			yield break;
		}
	}
}
