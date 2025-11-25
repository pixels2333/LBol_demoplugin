using System;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.EnemyUnits.Normal.Guihuos
{
	[UsedImplicitly]
	public sealed class GuihuoRed : Guihuo
	{
		protected override Type DebuffType
		{
			get
			{
				return typeof(Vulnerable);
			}
		}
		protected override string SkillVFX
		{
			get
			{
				return "GuihuoRskill";
			}
		}
	}
}
