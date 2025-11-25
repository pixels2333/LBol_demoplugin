using System;
namespace LBoL.Core
{
	public interface IMapModeOverrider
	{
		GameRunMapMode? MapMode { get; }
		void OnEnteredWithMode();
	}
}
