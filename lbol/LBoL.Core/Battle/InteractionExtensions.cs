using System;
namespace LBoL.Core.Battle
{
	public static class InteractionExtensions
	{
		public static TInteraction SetSource<TInteraction>(this TInteraction interaction, GameEntity source) where TInteraction : Interaction
		{
			interaction.Source = source;
			return interaction;
		}
	}
}
