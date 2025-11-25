using System;
using System.Collections;
namespace LBoL.Core.Battle
{
	public delegate IEnumerator BattleActionViewer<in TAction>(TAction action) where TAction : BattleAction;
}
