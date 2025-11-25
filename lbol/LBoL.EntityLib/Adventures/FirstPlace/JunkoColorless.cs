using System;
using System.Collections;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Battle.Interactions;
namespace LBoL.EntityLib.Adventures.FirstPlace
{
	public sealed class JunkoColorless : Adventure
	{
		[RuntimeCommand("convert", "")]
		[UsedImplicitly]
		public IEnumerator Convert(string desc)
		{
			ManaGroup baseMana = base.GameRun.BaseMana;
			int? num = new int?(0);
			int? num2 = new int?(0);
			ManaGroup manaGroup = baseMana.With(default(int?), default(int?), default(int?), default(int?), default(int?), default(int?), num, num2);
			SelectBaseManaInteraction interaction = new SelectBaseManaInteraction(1, 1, manaGroup)
			{
				Description = desc,
				CanCancel = false
			};
			yield return base.GameRun.InteractionViewer.View(interaction);
			ManaGroup selectedMana = interaction.SelectedMana;
			base.Storage.SetValue("$color", selectedMana.MaxColor.ToShortName().ToString());
			base.GameRun.SetBaseMana(base.GameRun.BaseMana - selectedMana + ManaGroup.Colorlesses(2), true);
			yield break;
		}
	}
}
