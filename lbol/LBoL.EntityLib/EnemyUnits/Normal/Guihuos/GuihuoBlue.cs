using System;
using JetBrains.Annotations;
namespace LBoL.EntityLib.EnemyUnits.Normal.Guihuos
{
	[UsedImplicitly]
	public sealed class GuihuoBlue : Guihuo
	{
		protected override string SkillVFX
		{
			get
			{
				return "GuihuoUskill";
			}
		}
	}
}
