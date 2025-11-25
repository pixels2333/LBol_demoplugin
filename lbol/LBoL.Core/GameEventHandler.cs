using System;
namespace LBoL.Core
{
	public delegate void GameEventHandler<in T>(T args) where T : GameEventArgs;
}
