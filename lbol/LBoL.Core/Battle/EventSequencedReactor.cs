using System;
using System.Collections.Generic;
namespace LBoL.Core.Battle
{
	public delegate IEnumerable<BattleAction> EventSequencedReactor<in TEventArgs>(TEventArgs args) where TEventArgs : GameEventArgs;
}
