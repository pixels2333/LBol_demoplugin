using System;
namespace LBoL.Core.Battle
{
	public interface ICustomCounter
	{
		CustomCounterResetTiming AutoResetTiming { get; }
		void Increase(BattleController battle);
		void Reset(BattleController battle);
	}
}
