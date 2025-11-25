using System;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.EnemyUnits.Normal.Guihuos
{
	[UsedImplicitly]
	public sealed class GuihuoGreen : Guihuo
	{
		protected override Type DebuffType
		{
			get
			{
				return typeof(Fragil);
			}
		}
		protected override string SkillVFX
		{
			get
			{
				return "GuihuoGskill";
			}
		}
	}
}
