using System;
using System.Collections;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Battle.Interactions;

namespace LBoL.EntityLib.Adventures.FirstPlace
{
	// Token: 0x02000520 RID: 1312
	public sealed class PatchouliPhilosophy : Adventure
	{
		// Token: 0x06001134 RID: 4404 RVA: 0x0001FD4C File Offset: 0x0001DF4C
		[RuntimeCommand("convert", "")]
		[UsedImplicitly]
		public IEnumerator Convert(string desc)
		{
			ManaGroup baseMana = base.GameRun.BaseMana;
			int? num = new int?(0);
			ManaGroup manaGroup = baseMana.With(default(int?), default(int?), default(int?), default(int?), default(int?), default(int?), default(int?), num);
			SelectBaseManaInteraction interaction = new SelectBaseManaInteraction(1, 1, manaGroup)
			{
				Description = desc,
				CanCancel = false
			};
			yield return base.GameRun.InteractionViewer.View(interaction);
			ManaGroup selectedMana = interaction.SelectedMana;
			base.Storage.SetValue("$selected", selectedMana.MaxColor.ToShortName().ToString());
			base.GameRun.SetBaseMana(base.GameRun.BaseMana - selectedMana + ManaGroup.Philosophies(1), true);
			yield break;
		}
	}
}
