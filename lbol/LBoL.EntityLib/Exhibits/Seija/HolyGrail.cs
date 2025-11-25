using System;
using JetBrains.Annotations;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;
namespace LBoL.EntityLib.Exhibits.Seija
{
	[UsedImplicitly]
	public sealed class HolyGrail : SeijaExhibit
	{
		protected override Type SeType
		{
			get
			{
				return typeof(HolyGrailSe);
			}
		}
	}
}
