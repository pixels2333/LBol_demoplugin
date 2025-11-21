using System;

namespace LBoL.Core.Battle
{
	// Token: 0x02000146 RID: 326
	public static class InteractionExtensions
	{
		// Token: 0x06000D22 RID: 3362 RVA: 0x00025226 File Offset: 0x00023426
		public static TInteraction SetSource<TInteraction>(this TInteraction interaction, GameEntity source) where TInteraction : Interaction
		{
			interaction.Source = source;
			return interaction;
		}
	}
}
