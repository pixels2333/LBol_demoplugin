using System;
using JetBrains.Annotations;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;
namespace LBoL.EntityLib.Exhibits.Seija
{
	[UsedImplicitly]
	public sealed class Shendeng : SeijaExhibit
	{
		protected override Type SeType
		{
			get
			{
				return typeof(ShendengSe);
			}
		}
		protected override void GenerateEffect()
		{
			base.GenerateEffect();
			this._effect.Level = 3;
		}
	}
}
