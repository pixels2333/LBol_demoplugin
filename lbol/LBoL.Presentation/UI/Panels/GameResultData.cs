using System;
using System.Collections.Generic;
using LBoL.Core;
namespace LBoL.Presentation.UI.Panels
{
	public class GameResultData
	{
		public string PlayerId { get; set; }
		public GameResultType Type { get; set; }
		public int PreviousTotalExp { get; set; }
		public int BluePoint { get; set; }
		public float DifficultyMultipler { get; set; }
		public List<ScoreData> ScoreDatas { get; set; }
		public int DebugExp { get; set; }
	}
}
