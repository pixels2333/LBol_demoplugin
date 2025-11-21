using System;
using System.Collections.Generic;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001A5 RID: 421
	public sealed class StartBattleAction : SimpleAction
	{
		// Token: 0x06000F2D RID: 3885 RVA: 0x00028E3F File Offset: 0x0002703F
		internal StartBattleAction()
		{
			this._args = new GameEventArgs
			{
				CanCancel = false
			};
		}

		// Token: 0x06000F2E RID: 3886 RVA: 0x00028E59 File Offset: 0x00027059
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreateEventPhase<GameEventArgs>("BattleStarting", this._args, base.Battle.BattleStarting);
			yield return base.CreatePhase("Main", new Action(base.Battle.StartBattle), true);
			yield return base.CreatePhase("Puzzle", delegate
			{
				if (base.Battle.GameRun.Puzzles.HasFlag(PuzzleFlag.NightMana))
				{
					Card card;
					switch (base.Battle.GameRun.CurrentStage.Level)
					{
					case 2:
						card = Library.CreateCard<NightMana2>();
						break;
					case 3:
						card = Library.CreateCard<NightMana3>();
						break;
					case 4:
						card = Library.CreateCard<NightMana4>();
						break;
					default:
						card = Library.CreateCard<NightMana1>();
						break;
					}
					Card card2 = card;
					base.React(new AddCardsToHandAction(new Card[] { card2 }), null, default(ActionCause?));
				}
			}, false);
			yield return base.CreateEventPhase<GameEventArgs>("BattleStarted", this._args, base.Battle.BattleStarted);
			yield break;
		}

		// Token: 0x0400069F RID: 1695
		private readonly GameEventArgs _args;
	}
}
