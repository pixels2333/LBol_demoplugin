using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class StartBattleAction : SimpleAction
	{
		internal StartBattleAction()
		{
			this._args = new GameEventArgs
			{
				CanCancel = false
			};
		}
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
		private readonly GameEventArgs _args;
	}
}
