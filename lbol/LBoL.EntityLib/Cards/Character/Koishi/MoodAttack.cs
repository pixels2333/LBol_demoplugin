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
	// Token: 0x02000485 RID: 1157
	[UsedImplicitly]
	public sealed class MoodAttack : Card
	{
		// Token: 0x06000F7A RID: 3962 RVA: 0x0001BA70 File Offset: 0x00019C70
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

		// Token: 0x06000F7B RID: 3963 RVA: 0x0001BAE5 File Offset: 0x00019CE5
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<MoodChangeEventArgs>(base.Battle.Player.MoodChanged, new EventSequencedReactor<MoodChangeEventArgs>(this.OnPlayerMoodChanged));
		}

		// Token: 0x06000F7C RID: 3964 RVA: 0x0001BB09 File Offset: 0x00019D09
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
