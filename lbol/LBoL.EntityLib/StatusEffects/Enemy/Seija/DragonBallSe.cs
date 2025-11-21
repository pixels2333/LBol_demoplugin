using System;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.Exhibits.Seija;

namespace LBoL.EntityLib.StatusEffects.Enemy.Seija
{
	// Token: 0x020000CC RID: 204
	public sealed class DragonBallSe : SeijaSe
	{
		// Token: 0x17000045 RID: 69
		// (get) Token: 0x060002C4 RID: 708 RVA: 0x0000783A File Offset: 0x00005A3A
		protected override Type ExhibitType
		{
			get
			{
				return typeof(DragonBall);
			}
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x00007848 File Offset: 0x00005A48
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			this.React(PerformAction.Sfx("GuirenItem", 0f));
			this.React(PerformAction.EffectMessage(unit, "SeijaExhibitManager", "AddExhibit", this));
			base.HandleOwnerEvent<DieEventArgs>(base.Owner.Dying, new GameEventHandler<DieEventArgs>(this.OnOwnerDying));
			base.Highlight = true;
		}

		// Token: 0x060002C6 RID: 710 RVA: 0x000078B6 File Offset: 0x00005AB6
		private void OnOwnerDying(DieEventArgs args)
		{
			if (base.Owner is Seija && base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>())
			{
				base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.SeijaSpecial);
			}
		}
	}
}
