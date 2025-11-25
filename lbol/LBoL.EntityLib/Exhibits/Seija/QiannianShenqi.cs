using System;
using JetBrains.Annotations;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;
namespace LBoL.EntityLib.Exhibits.Seija
{
	[UsedImplicitly]
	public sealed class QiannianShenqi : SeijaExhibit
	{
		protected override Type SeType
		{
			get
			{
				return typeof(QiannianShenqiSe);
			}
		}
		protected override void GenerateEffect()
		{
			base.GenerateEffect();
			this._effect.Level = 2;
			this._effect.Limit = 10;
		}
	}
}
