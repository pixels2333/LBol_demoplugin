using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;
namespace LBoL.EntityLib.Cards.Character.Koishi
{
	[UsedImplicitly]
	public sealed class MoodAttack : Card
	{
		protected override void SetGuns()
		{
			string text = "移情刃";
			string text2 = "0";
			if (base.Battle.Player.HasStatusEffect<MoodPassion>())
			{
				text2 = "1";
			}
			if (base.Battle.Player.HasStatusEffect<MoodPeace>())
			{
				text2 = "2";
			}
			if (base.Battle.Player.HasStatusEffect<MoodEpiphany>())
			{
				text2 = "3";
			}
			text += text2;
			base.CardGuns = new Guns(text);
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<MoodChangeEventArgs>(base.Battle.Player.MoodChanged, new EventSequencedReactor<MoodChangeEventArgs>(this.OnPlayerMoodChanged));
		}
		private IEnumerable<BattleAction> OnPlayerMoodChanged(MoodChangeEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Zone == CardZone.Discard && base.Battle.HandIsNotFull)
			{
				yield return new MoveCardAction(this, CardZone.Hand);
			}
			yield break;
		}
	}
}
