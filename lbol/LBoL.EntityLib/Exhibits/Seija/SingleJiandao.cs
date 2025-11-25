using System;
using JetBrains.Annotations;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;
namespace LBoL.EntityLib.Exhibits.Seija
{
	[UsedImplicitly]
	public sealed class SingleJiandao : SeijaExhibit
	{
		protected override Type SeType
		{
			get
			{
				return typeof(SingleJiandaoSe);
			}
		}
		protected override void GenerateEffect()
		{
			base.GenerateEffect();
			this._effect.Level = 1;
		}
	}
}
