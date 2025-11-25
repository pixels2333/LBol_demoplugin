using System;
namespace LBoL.Core.Battle
{
	public enum ActionCause
	{
		None,
		Card,
		CardUse,
		AutoExile,
		Us,
		UsUse,
		Doll,
		StatusEffect,
		Exhibit,
		JadeBox,
		BattleStart,
		BattleEnd,
		TurnStart,
		TurnEnd,
		RoundStart,
		RoundEnd,
		Player,
		EnemyAction,
		Unit,
		Gap,
		OnlyCalculate
	}
}
