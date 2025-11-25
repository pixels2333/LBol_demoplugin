using System;
using System.Collections;
using LBoL.Core.Battle;
namespace LBoL.Core
{
	public delegate IEnumerator InteractionViewer<in TInteraction>(TInteraction interaction) where TInteraction : Interaction;
}
