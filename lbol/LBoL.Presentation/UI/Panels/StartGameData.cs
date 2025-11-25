using System;
using LBoL.Core;
namespace LBoL.Presentation.UI.Panels
{
	public class StartGameData
	{
		public Func<Stage[]> StagesCreateFunc { get; set; }
		public Type DebutAdventure { get; set; }
	}
}
