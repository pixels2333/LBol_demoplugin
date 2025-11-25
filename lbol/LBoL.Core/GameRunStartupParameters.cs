using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LBoL.Core.Cards;
using LBoL.Core.SaveData;
using LBoL.Core.Units;
namespace LBoL.Core
{
	public sealed class GameRunStartupParameters
	{
		public GameMode Mode { get; set; }
		public bool ShowRandomResult { get; set; }
		public ulong? Seed { get; set; }
		public GameDifficulty Difficulty { get; set; }
		public PuzzleFlag Puzzles { get; set; }
		public PlayerUnit Player { get; set; }
		public PlayerType PlayerType { get; set; }
		public Exhibit InitExhibit { get; set; }
		public int? InitMoneyOverride { get; set; }
		public IEnumerable<Card> Deck { get; set; }
		public IEnumerable<Stage> Stages { get; set; }
		public Type DebutAdventureType
		{
			[return: MaybeNull]
			get;
			set; }
		public IEnumerable<JadeBox> JadeBoxes { get; set; }
		public ProfileSaveData UserProfile { get; set; }
		public int UnlockLevel { get; set; }
	}
}
