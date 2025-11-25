using System;
using JetBrains.Annotations;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;
namespace LBoL.EntityLib.Exhibits.Seija
{
	[UsedImplicitly]
	public sealed class InfinityGems : SeijaExhibit
	{
		protected override Type SeType
		{
			get
			{
				return typeof(InfinityGemsSe);
			}
		}
		protected override void GenerateEffect()
		{
			base.GenerateEffect();
			this._effect.Count = 2;
		}
	}
}
